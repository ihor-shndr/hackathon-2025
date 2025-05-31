#!/bin/bash

# Script to get the current app URL via Load Balancer
set -e

echo "🔍 Getting app URL..."

# Check if AWS credentials are configured
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ AWS credentials not configured. Please run 'aws configure' or set AWS environment variables."
    exit 1
fi

# Get the load balancer DNS name from terraform output
LB_DNS=$(terraform output -raw load_balancer_dns 2>/dev/null)

if [ -z "$LB_DNS" ]; then
    echo "❌ Could not get load balancer DNS name from terraform output"
    echo "   Make sure terraform has been applied successfully"
    exit 1
fi

echo "✅ Your MyChat app is running at:"
echo "   Frontend: http://$LB_DNS"
echo "   Backend API: http://$LB_DNS:8080"
echo ""
echo "📝 This URL is stable and won't change when the Fargate task restarts"
echo ""
echo "🔍 To view logs: ./view-logs.sh [container-name]"
echo "   Available containers: frontend, backend, postgres" 