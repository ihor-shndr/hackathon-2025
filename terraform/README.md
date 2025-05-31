# MyChat App - Ultra-Simple Docker-Compose on Fargate

The simplest possible setup: your entire `docker-compose.yml` running as one Fargate task with public access.

## What This Does

✅ **Runs your exact docker-compose stack**: frontend + backend + postgres  
✅ **One Fargate task**: All containers together, like `docker-compose up`  
✅ **Public access**: Direct access via `http://ip:3000` and `http://ip:8080`  
✅ **Persistent data**: PostgreSQL data stored on EFS  
✅ **S3 images**: Chat images stored in S3 bucket  
✅ **No logs**: Minimal setup, no CloudWatch logging  
✅ **No complexity**: Just VPC + Fargate + S3 + EFS  

## Prerequisites

- AWS account with credentials configured
- Terraform installed
- Docker installed

## Quick Start

### 1. Configure

```bash
cd terraform
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars: set db_password and jwt_secret
```

### 2. Deploy Infrastructure

```bash
./deploy.sh
```

### 3. Build and Deploy Apps

```bash
./push-images.sh
```

This builds and pushes both frontend and backend Docker images to ECR, then updates the Fargate service.

### 4. Get Your App URL

```bash
./get-app-url.sh
```

Shows you the current public IP to access your app.

## Usage

Your entire app runs at one IP address:
- **Frontend**: `http://<ip>:3000` 
- **Backend API**: `http://<ip>:8080`

The IP changes when the Fargate task restarts, so use `./get-app-url.sh` to get the current one.

## What Gets Created

- **VPC + Subnets**: Basic networking
- **S3 Bucket**: For chat images
- **ECR Repositories**: For frontend + backend images  
- **EFS**: For PostgreSQL data persistence
- **Fargate Task**: Running all 3 containers
- **Security Group**: Allows ports 3000 and 8080

## Container Setup

All containers run in one Fargate task, just like your local `docker-compose.yml`:

1. **PostgreSQL**: Database with data on EFS
2. **Backend**: .NET API connecting to localhost postgres
3. **Frontend**: React app calling localhost:8080 API

## Costs

Very low cost (~$10-15/month):
- Fargate: 1024 CPU, 2048 MB memory
- EFS: Only pay for data stored  
- S3: Standard storage for images
- No NAT Gateway, no RDS, no logs

## Commands

```bash
# Get current app URL
./get-app-url.sh

# Redeploy after code changes
./push-images.sh

# Check service status
aws ecs describe-services --cluster mychat-app-cluster --services mychat-app-service

# Destroy everything
./destroy.sh
```

## Limitations

- **Single task**: No high availability
- **IP changes**: When task restarts  
- **HTTP only**: No HTTPS/SSL
- **No logs**: No CloudWatch logging
- **Basic setup**: Minimal security

Perfect for testing and development!

## Troubleshooting

### Service not starting
```bash
aws ecs describe-services --cluster mychat-app-cluster --services mychat-app-service
```

### Can't get IP
```bash
aws ecs list-tasks --cluster mychat-app-cluster --service-name mychat-app-service
```

### Images not pushing
Check that ECR repositories exist:
```bash
terraform output frontend_ecr_repository_url
terraform output backend_ecr_repository_url
``` 