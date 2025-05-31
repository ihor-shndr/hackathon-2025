#!/bin/bash

# Script to build and push Docker images to ECR and deploy
set -e

echo "🚀 Building and deploying full docker-compose stack to Fargate..."

# Check if AWS credentials are configured
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ AWS credentials not configured. Please run 'aws configure' or set AWS environment variables."
    exit 1
fi

# Get repository URLs from Terraform output
echo "📋 Getting deployment information..."
FRONTEND_REPO=$(terraform output -raw frontend_ecr_repository_url)
BACKEND_REPO=$(terraform output -raw backend_ecr_repository_url)
AWS_REGION=$(terraform output -raw aws_region || echo "us-east-1")
LOAD_BALANCER_DNS=$(terraform output -raw load_balancer_dns)

if [ -z "$FRONTEND_REPO" ] || [ -z "$BACKEND_REPO" ]; then
    echo "❌ Could not get ECR repository URLs. Make sure Terraform has been applied successfully."
    exit 1
fi

echo "Frontend repo: $FRONTEND_REPO"
echo "Backend repo: $BACKEND_REPO"
echo "Load balancer DNS: $LOAD_BALANCER_DNS"

# Get ECR login token
echo "🔐 Logging into ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $FRONTEND_REPO

# Build and push frontend image for Linux AMD64 platform with correct API URL
echo "🏗️  Building frontend image for Linux AMD64 with API URL: http://$LOAD_BALANCER_DNS:8080"
cd ../frontend
docker build --platform linux/amd64 --build-arg REACT_APP_API_URL=http://$LOAD_BALANCER_DNS:8080 -t mychat-frontend .
docker tag mychat-frontend:latest $FRONTEND_REPO:latest

echo "📤 Pushing frontend image..."
docker push $FRONTEND_REPO:latest

# Build and push backend image for Linux AMD64 platform
echo "🏗️  Building backend image for Linux AMD64..."
cd ../backend
docker build --platform linux/amd64 -t mychat-backend .
docker tag mychat-backend:latest $BACKEND_REPO:latest

echo "📤 Pushing backend image..."
docker push $BACKEND_REPO:latest

cd ../terraform

echo "✅ Images pushed successfully!"
echo ""
echo "🔄 Updating ECS service with new images..."

# Force new deployment of the ECS service
aws ecs update-service --cluster mychat-app-cluster --service mychat-app-service --force-new-deployment --region $AWS_REGION

echo "✅ ECS service is updating!"
echo ""
echo "🔧 Wait a few minutes for the service to start, then get your app URL with:"
echo "   ./get-app-url.sh"
echo ""
echo "📱 Your app will be available at: http://<public-ip>:3000" 