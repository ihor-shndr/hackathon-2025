#!/bin/bash

# Script to get the current Fargate public IP for API access
set -e

echo "🔍 Getting current Fargate API endpoint..."

# Check if AWS credentials are configured
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ AWS credentials not configured. Please run 'aws configure' or set AWS environment variables."
    exit 1
fi

AWS_REGION=$(terraform output -raw aws_region || echo "us-east-1")
CLUSTER_NAME="mychat-app-cluster"
SERVICE_NAME="mychat-app-service"

# Get the task ARN
TASK_ARN=$(aws ecs list-tasks --cluster $CLUSTER_NAME --service-name $SERVICE_NAME --query 'taskArns[0]' --output text --region $AWS_REGION)

if [ "$TASK_ARN" = "None" ] || [ -z "$TASK_ARN" ]; then
    echo "❌ No running tasks found for service $SERVICE_NAME"
    echo "   Make sure the ECS service is running"
    exit 1
fi

# Get the ENI ID from the task
ENI_ID=$(aws ecs describe-tasks --cluster $CLUSTER_NAME --tasks $TASK_ARN --query 'tasks[0].attachments[0].details[?name==`networkInterfaceId`].value' --output text --region $AWS_REGION)

if [ -z "$ENI_ID" ]; then
    echo "❌ Could not get network interface ID"
    exit 1
fi

# Get the public IP from the ENI
PUBLIC_IP=$(aws ec2 describe-network-interfaces --network-interface-ids $ENI_ID --query 'NetworkInterfaces[0].Association.PublicIp' --output text --region $AWS_REGION)

if [ -z "$PUBLIC_IP" ] || [ "$PUBLIC_IP" = "None" ]; then
    echo "❌ Could not get public IP address"
    echo "   The task might still be starting up or doesn't have a public IP"
    exit 1
fi

echo "✅ Backend API is accessible at:"
echo "   http://$PUBLIC_IP:8080"
echo ""
echo "📝 You can use this URL to configure your frontend API calls"
echo "   Example: REACT_APP_API_URL=http://$PUBLIC_IP:8080" 