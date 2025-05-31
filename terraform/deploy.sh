#!/bin/bash

# Terraform deployment script for mychat-app infrastructure
set -e

echo "🚀 Starting Terraform deployment for mychat-app..."

# Check if AWS credentials are configured
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ AWS credentials not configured. Please run 'aws configure' or set AWS environment variables."
    exit 1
fi

# Check if terraform.tfvars exists
if [ ! -f "terraform.tfvars" ]; then
    echo "📝 Creating terraform.tfvars from example..."
    cp terraform.tfvars.example terraform.tfvars
    echo "⚠️  Please edit terraform.tfvars and set your passwords before continuing!"
    echo "   Required variables: db_password, jwt_secret"
    read -p "Press Enter after you've updated terraform.tfvars..."
fi

# Initialize Terraform
echo "🔧 Initializing Terraform..."
terraform init

# Plan the deployment
echo "📋 Planning Terraform deployment..."
terraform plan -out=tfplan

# Ask for confirmation
echo "📋 Terraform plan created. Review the above output."
read -p "Do you want to apply this plan? (yes/no): " confirm

if [ "$confirm" = "yes" ]; then
    # Apply the plan
    echo "🚀 Applying Terraform configuration..."
    terraform apply tfplan
    
    echo "✅ Infrastructure deployment completed!"
    echo ""
    echo "📋 Important outputs:"
    terraform output
    
    echo ""
    echo "🔧 Next steps:"
    echo "1. Build and push your Docker images to the ECR repositories"
    echo "2. Update ECS services to use the new images"
    echo "3. Your application will be available at the load balancer DNS name"
    
else
    echo "❌ Deployment cancelled."
    rm -f tfplan
fi 