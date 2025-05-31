# AWS Provider Configuration
terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# Variables
variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "mychat-app"
}

variable "db_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT secret key"
  type        = string
  sensitive   = true
}

# Data sources
data "aws_availability_zones" "available" {
  state = "available"
}

# Use existing VPC instead of creating a new one
data "aws_vpc" "main" {
  id = "vpc-0a93fabd94dc8ed7e"
}

# Get public subnets from the existing VPC
data "aws_subnets" "public" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.main.id]
  }
  
  filter {
    name   = "map-public-ip-on-launch"
    values = ["true"]
  }
}

# Get individual subnet details
data "aws_subnet" "public" {
  for_each = toset(data.aws_subnets.public.ids)
  id       = each.value
}

# CloudWatch Log Groups for ECS logging
resource "aws_cloudwatch_log_group" "postgres" {
  name              = "/ecs/${var.app_name}/postgres"
  retention_in_days = 7

  tags = {
    Name  = "${var.app_name}-postgres-logs"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_cloudwatch_log_group" "backend" {
  name              = "/ecs/${var.app_name}/backend"
  retention_in_days = 7

  tags = {
    Name  = "${var.app_name}-backend-logs"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_cloudwatch_log_group" "frontend" {
  name              = "/ecs/${var.app_name}/frontend"
  retention_in_days = 7

  tags = {
    Name  = "${var.app_name}-frontend-logs"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# S3 Bucket for image storage only (private access through API)
resource "aws_s3_bucket" "images" {
  bucket = "${var.app_name}-images-${random_string.bucket_suffix.result}"

  tags = {
    Name  = "${var.app_name}-images"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "random_string" "bucket_suffix" {
  length  = 8
  special = false
  upper   = false
}

# Block all public access to S3 bucket - images served through API only
resource "aws_s3_bucket_public_access_block" "images_pab" {
  bucket = aws_s3_bucket.images.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# Security Group - Allow HTTP, HTTPS, and Backend API traffic to ALB
resource "aws_security_group" "alb" {
  name_prefix = "${var.app_name}-alb-"
  vpc_id      = data.aws_vpc.main.id

  # HTTP access from internet
  ingress {
    protocol    = "tcp"
    from_port   = 80
    to_port     = 80
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTPS access from internet (for future SSL)
  ingress {
    protocol    = "tcp"
    from_port   = 443
    to_port     = 443
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Backend API access from internet
  ingress {
    protocol    = "tcp"
    from_port   = 8080
    to_port     = 8080
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name  = "${var.app_name}-alb-sg"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# Security Group - Allow frontend and backend ports from ALB
resource "aws_security_group" "app" {
  name_prefix = "${var.app_name}-app-"
  vpc_id      = data.aws_vpc.main.id

  # Frontend port from ALB
  ingress {
    protocol        = "tcp"
    from_port       = 3000
    to_port         = 3000
    security_groups = [aws_security_group.alb.id]
  }

  # Backend API port from ALB
  ingress {
    protocol        = "tcp"
    from_port       = 8080
    to_port         = 8080
    security_groups = [aws_security_group.alb.id]
  }

  egress {
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name  = "${var.app_name}-app-sg"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# Application Load Balancer
resource "aws_lb" "main" {
  name               = "${var.app_name}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = data.aws_subnets.public.ids

  enable_deletion_protection = false

  tags = {
    Name  = "${var.app_name}-alb"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# Target Groups
resource "aws_lb_target_group" "frontend" {
  name        = "${var.app_name}-frontend-tg"
  port        = 3000
  protocol    = "HTTP"
  vpc_id      = data.aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = {
    Name  = "${var.app_name}-frontend-tg"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_lb_target_group" "backend" {
  name        = "${var.app_name}-backend-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = data.aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = {
    Name  = "${var.app_name}-backend-tg"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# Load Balancer Listeners
resource "aws_lb_listener" "frontend" {
  load_balancer_arn = aws_lb.main.arn
  port              = "80"
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.frontend.arn
  }

  tags = {
    Name  = "${var.app_name}-frontend-listener"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_lb_listener" "backend" {
  load_balancer_arn = aws_lb.main.arn
  port              = "8080"
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }

  tags = {
    Name  = "${var.app_name}-backend-listener"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# ECS Cluster
resource "aws_ecs_cluster" "main" {
  name = "${var.app_name}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = {
    Name  = "${var.app_name}-cluster"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# IAM Roles for ECS
resource "aws_iam_role" "ecs_task_execution_role" {
  name = "${var.app_name}-ecs-task-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name  = "${var.app_name}-ecs-task-execution-role"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_policy" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# Additional policy for CloudWatch logs
resource "aws_iam_role_policy" "ecs_task_execution_logs_policy" {
  name = "${var.app_name}-ecs-task-execution-logs-policy"
  role = aws_iam_role.ecs_task_execution_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = "arn:aws:logs:${var.aws_region}:*:*"
      }
    ]
  })
}

resource "aws_iam_role" "ecs_task_role" {
  name = "${var.app_name}-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name  = "${var.app_name}-ecs-task-role"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_iam_role_policy" "ecs_task_s3_policy" {
  name = "${var.app_name}-ecs-task-s3-policy"
  role = aws_iam_role.ecs_task_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject"
        ]
        Resource = "${aws_s3_bucket.images.arn}/*"
      },
      {
        Effect = "Allow"
        Action = [
          "s3:ListBucket"
        ]
        Resource = aws_s3_bucket.images.arn
      }
    ]
  })
}

# ECR Repositories
resource "aws_ecr_repository" "frontend" {
  name                 = "${var.app_name}-frontend"
  image_tag_mutability = "MUTABLE"

  tags = {
    Name  = "${var.app_name}-frontend"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_ecr_repository" "backend" {
  name                 = "${var.app_name}-backend"
  image_tag_mutability = "MUTABLE"

  tags = {
    Name  = "${var.app_name}-backend"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# EFS for PostgreSQL data persistence
resource "aws_efs_file_system" "postgres_data" {
  creation_token = "${var.app_name}-postgres-data"
  
  tags = {
    Name  = "${var.app_name}-postgres-data"
    Owner = "ihor.shnaider@dataart.com"
  }
}

resource "aws_efs_mount_target" "postgres_data" {
  count          = length(data.aws_subnets.public.ids)
  file_system_id = aws_efs_file_system.postgres_data.id
  subnet_id      = data.aws_subnets.public.ids[count.index]
  security_groups = [aws_security_group.efs.id]
}

resource "aws_security_group" "efs" {
  name_prefix = "${var.app_name}-efs-"
  vpc_id      = data.aws_vpc.main.id

  ingress {
    protocol        = "tcp"
    from_port       = 2049
    to_port         = 2049
    security_groups = [aws_security_group.app.id]
  }

  egress {
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name  = "${var.app_name}-efs-sg"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# ECS Task Definition - Full docker-compose stack
resource "aws_ecs_task_definition" "app" {
  family                   = "${var.app_name}"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = 2048
  memory                   = 4096
  execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
  task_role_arn           = aws_iam_role.ecs_task_role.arn

  volume {
    name = "postgres-data"
    efs_volume_configuration {
      file_system_id = aws_efs_file_system.postgres_data.id
      root_directory = "/"
    }
  }

  container_definitions = jsonencode([
    {
      name  = "postgres"
      image = "postgres:16-alpine"
      essential = true
      environment = [
        {
          name  = "POSTGRES_DB"
          value = "mychat"
        },
        {
          name  = "POSTGRES_USER"
          value = "postgres"
        },
        {
          name  = "POSTGRES_PASSWORD"
          value = var.db_password
        },
        {
          name  = "POSTGRES_HOST_AUTH_METHOD"
          value = "trust"
        }
      ]
      portMappings = [
        {
          containerPort = 5432
          protocol      = "tcp"
        }
      ]
      mountPoints = [
        {
          sourceVolume  = "postgres-data"
          containerPath = "/var/lib/postgresql/data"
        }
      ]
      healthCheck = {
        command = ["CMD-SHELL", "pg_isready -U postgres -d mychat"]
        interval = 30
        timeout = 5
        retries = 3
        startPeriod = 60
      }
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.postgres.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }
    },
    {
      name  = "backend"
      image = "${aws_ecr_repository.backend.repository_url}:latest"
      essential = true
      portMappings = [
        {
          containerPort = 8080
          protocol      = "tcp"
        }
      ]
      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:8080"
        },
        {
          name  = "ConnectionStrings__DefaultConnection"
          value = "Host=localhost;Database=mychat;Username=postgres;Password=${var.db_password}"
        },
        {
          name  = "JwtSettings__SecretKey"
          value = var.jwt_secret
        },
        {
          name  = "AWS__S3__BucketName"
          value = aws_s3_bucket.images.id
        },
        {
          name  = "AWS__S3__Region"
          value = var.aws_region
        }
      ]
      dependsOn = [
        {
          containerName = "postgres"
          condition     = "HEALTHY"
        }
      ]
      healthCheck = {
        command = ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
        interval = 30
        timeout = 5
        retries = 3
        startPeriod = 120
      }
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.backend.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }
    },
    {
      name  = "frontend"
      image = "${aws_ecr_repository.frontend.repository_url}:latest"
      essential = true
      portMappings = [
        {
          containerPort = 3000
          protocol      = "tcp"
        }
      ]
      # No runtime environment needed - API URL is baked into build
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.frontend.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }
    }
  ])

  tags = {
    Name  = "${var.app_name}-task-definition"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# ECS Service
resource "aws_ecs_service" "app" {
  name            = "${var.app_name}-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    security_groups  = [aws_security_group.app.id]
    subnets          = data.aws_subnets.public.ids
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.frontend.arn
    container_name   = "frontend"
    container_port   = 3000
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.backend.arn
    container_name   = "backend"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.frontend, aws_lb_listener.backend]

  tags = {
    Name  = "${var.app_name}-service"
    Owner = "ihor.shnaider@dataart.com"
  }
}

# Outputs
output "app_access_info" {
  description = "How to access your application"
  value = <<EOF
Your MyChat app is running!
  Frontend: http://${aws_lb.main.dns_name}
  Backend API: http://${aws_lb.main.dns_name}:8080
  
CloudWatch Log Groups:
  - Frontend: /ecs/${var.app_name}/frontend
  - Backend: /ecs/${var.app_name}/backend  
  - PostgreSQL: /ecs/${var.app_name}/postgres
EOF
}

output "load_balancer_dns" {
  description = "Load balancer DNS name"
  value       = aws_lb.main.dns_name
}

output "s3_images_bucket" {
  description = "S3 bucket name for images (private - access via API only)"
  value       = aws_s3_bucket.images.id
}

output "frontend_ecr_repository_url" {
  description = "URL of the frontend ECR repository"
  value       = aws_ecr_repository.frontend.repository_url
}

output "backend_ecr_repository_url" {
  description = "URL of the backend ECR repository"
  value       = aws_ecr_repository.backend.repository_url
}

output "efs_file_system_id" {
  description = "EFS file system ID for PostgreSQL data"
  value       = aws_efs_file_system.postgres_data.id
}

# Additional outputs needed for get-app-url.sh script
output "aws_region" {
  description = "AWS region"
  value       = var.aws_region
}

output "cluster_name" {
  description = "ECS cluster name"
  value       = aws_ecs_cluster.main.name
}

output "service_name" {
  description = "ECS service name"
  value       = aws_ecs_service.app.name
}

output "cloudwatch_log_groups" {
  description = "CloudWatch log group names for debugging"
  value = {
    frontend = aws_cloudwatch_log_group.frontend.name
    backend  = aws_cloudwatch_log_group.backend.name
    postgres = aws_cloudwatch_log_group.postgres.name
  }
}
