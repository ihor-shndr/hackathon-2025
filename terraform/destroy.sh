#!/bin/bash

# Terraform destroy script for mychat-app infrastructure
set -e

echo "🗑️  Starting Terraform destroy for mychat-app infrastructure..."

# Check if AWS credentials are configured
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ AWS credentials not configured. Please run 'aws configure' or set AWS environment variables."
    exit 1
fi

# Check if terraform state exists
if [ ! -f "terraform.tfstate" ] && [ ! -f ".terraform/terraform.tfstate" ]; then
    echo "❌ No Terraform state found. Nothing to destroy."
    exit 1
fi

# Warning message
echo "⚠️  WARNING: This will destroy ALL infrastructure resources!"
echo "   - S3 bucket and all images"
echo "   - RDS database and all data"
echo "   - ECS cluster and running services"
echo "   - VPC and all networking components"
echo ""

read -p "Are you sure you want to destroy all resources? Type 'destroy' to confirm: " confirm

if [ "$confirm" = "destroy" ]; then
    echo "🗑️  Destroying infrastructure..."
    terraform destroy -auto-approve
    
    echo "✅ Infrastructure destruction completed!"
    echo "📋 All AWS resources have been removed."
else
    echo "❌ Destruction cancelled."
fi 