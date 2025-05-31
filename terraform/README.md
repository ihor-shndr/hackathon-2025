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

# MyChat Terraform Infrastructure

This Terraform configuration deploys a complete MyChat application stack on AWS using Fargate with CloudWatch logging and Application Load Balancer.

## 🏗️ Architecture

- **ECS Fargate**: Runs PostgreSQL, Backend API (.NET), and Frontend (React) containers
- **Application Load Balancer**: Provides stable endpoints for frontend and backend
- **CloudWatch**: Centralized logging for all containers with 7-day retention
- **EFS**: Persistent storage for PostgreSQL data
- **ECR**: Container registries for frontend and backend images
- **S3**: Private bucket for image storage (API access only)
- **VPC**: Custom networking with public subnets

## 🚀 Quick Start

1. **Set your variables** in `terraform.tfvars`:
   ```bash
   db_password = "your-secure-password"
   jwt_secret  = "your-jwt-secret-key"
   ```

2. **Deploy infrastructure**:
   ```bash
   ./deploy.sh
   ```

3. **Build and push your Docker images**:
   ```bash
   ./push-images.sh
   ```

4. **Get your application URL**:
   ```bash
   ./get-app-url.sh
   ```

## 📊 Monitoring & Debugging

### View Logs
```bash
# View backend logs
./view-logs.sh backend

# View frontend logs  
./view-logs.sh frontend

# View PostgreSQL logs
./view-logs.sh postgres
```

### Troubleshoot Issues
```bash
# Comprehensive health check
./troubleshoot.sh
```

This will check:
- ✅ ECS service status
- ✅ Task health and status
- ✅ Load balancer health
- ✅ Target group health
- ✅ Recent CloudWatch logs
- ✅ Common solutions

### CloudWatch Log Groups
- **Frontend**: `/ecs/mychat-app/frontend`
- **Backend**: `/ecs/mychat-app/backend`
- **PostgreSQL**: `/ecs/mychat-app/postgres`

## 🌐 Access Your Application

After deployment, your app will be available at:
- **Frontend**: `http://[load-balancer-dns]`
- **Backend API**: `http://[load-balancer-dns]:8080`

The load balancer DNS is stable and won't change when containers restart.

## 🔧 Common Issues & Solutions

### 1. Containers Not Starting
**Symptoms**: Running count is 0, tasks keep stopping
**Solutions**:
- Check logs: `./view-logs.sh [container-name]`
- Verify Docker images exist in ECR
- Check environment variables in task definition

### 2. Load Balancer Health Checks Failing
**Symptoms**: Targets showing as unhealthy
**Solutions**:
- Ensure your app responds to health check paths:
  - Frontend: `GET /` should return 200
  - Backend: `GET /health` should return 200
- Check container port mappings
- Verify security groups allow traffic

### 3. Can't See Logs
**Symptoms**: No logs in CloudWatch
**Solutions**:
- Wait a few minutes for logs to appear
- Check ECS task execution role has CloudWatch permissions
- Verify log groups exist in CloudWatch console

### 4. Database Connection Issues
**Symptoms**: Backend can't connect to PostgreSQL
**Solutions**:
- Check PostgreSQL logs: `./view-logs.sh postgres`
- Verify EFS mount is working
- Check database environment variables

### 5. Frontend Can't Reach Backend
**Symptoms**: API calls failing from frontend
**Solutions**:
- Verify `REACT_APP_API_URL` points to load balancer
- Check backend is responding on port 8080
- Test backend directly: `curl http://[lb-dns]:8080/health`

## 🗂️ File Structure

```
terraform/
├── main.tf              # Main infrastructure configuration
├── terraform.tfvars     # Variables (set your passwords here)
├── deploy.sh           # Deployment script
├── destroy.sh          # Cleanup script
├── get-app-url.sh      # Get current app URL
├── view-logs.sh        # View container logs
├── troubleshoot.sh     # Comprehensive diagnostics
├── push-images.sh      # Build and push Docker images
└── README.md           # This file
```

## 🛠️ Manual Operations

### View ECS Service Status
```bash
aws ecs describe-services \
  --cluster mychat-app-cluster \
  --services mychat-app-service \
  --region us-east-1
```

### Force New Deployment
```bash
aws ecs update-service \
  --cluster mychat-app-cluster \
  --service mychat-app-service \
  --force-new-deployment \
  --region us-east-1
```

### Check Target Group Health
```bash
aws elbv2 describe-target-health \
  --target-group-arn [target-group-arn] \
  --region us-east-1
```

## 🔄 Updates & Redeployment

To update your application:

1. **Update code** and build new images:
   ```bash
   ./push-images.sh
   ```

2. **Force new deployment** (automatically picks up latest images):
   ```bash
   aws ecs update-service --cluster mychat-app-cluster --service mychat-app-service --force-new-deployment --region us-east-1
   ```

3. **Monitor deployment**:
   ```bash
   ./troubleshoot.sh
   ```

## 💰 Cost Optimization

- **Fargate**: ~$30-50/month for 1 vCPU, 2GB RAM running 24/7
- **ALB**: ~$16/month base cost
- **CloudWatch Logs**: ~$0.50/GB ingested
- **EFS**: ~$0.30/GB stored
- **S3**: ~$0.023/GB stored

To reduce costs:
- Use `desired_count = 0` when not testing
- Set shorter log retention periods
- Use S3 lifecycle policies for old images

## 🧹 Cleanup

To destroy all resources:
```bash
./destroy.sh
```

**Warning**: This will permanently delete all data including the PostgreSQL database and uploaded images.

## 📞 Support

If you encounter issues:

1. Run `./troubleshoot.sh` for automated diagnostics
2. Check CloudWatch logs with `./view-logs.sh [container]`
3. Review ECS service events in AWS console
4. Verify security group rules and network configuration

## 🔐 Security Notes

- S3 bucket blocks all public access (images served via API)
- PostgreSQL only accessible within VPC
- Security groups restrict access to necessary ports only
- Sensitive variables (passwords, JWT secret) stored securely
- CloudWatch logs for audit trail 