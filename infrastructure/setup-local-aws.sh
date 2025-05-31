#!/bin/bash

# Setup script for local Docker Compose with AWS credentials
echo "🔧 Setting up local environment with AWS credentials..."

# Check if AWS CLI is configured (try multiple methods for temporary credentials)
echo "🔍 Checking AWS CLI configuration..."

# First try to get caller identity
if aws sts get-caller-identity &> /dev/null; then
    echo "✅ AWS CLI is configured and working with STS"
elif aws configure get aws_access_key_id &> /dev/null; then
    echo "✅ AWS CLI has credentials configured"
    echo "⚠️  Note: STS check failed (normal for some credential types)"
else
    echo "❌ AWS CLI not configured. Please run 'aws configure' first."
    echo "💡 Make sure you have set:"
    echo "   - AWS Access Key ID"
    echo "   - AWS Secret Access Key"
    echo "   - Default region (us-east-1 recommended)"
    exit 1
fi

# Use existing environment variables if available, otherwise fall back to aws configure
echo "📤 Using AWS credentials..."
if [[ -n "$AWS_ACCESS_KEY_ID" && -n "$AWS_SECRET_ACCESS_KEY" ]]; then
    echo "✅ Using credentials from environment variables"
    # Environment variables are already set, just make sure they're exported
    export AWS_ACCESS_KEY_ID="$AWS_ACCESS_KEY_ID"
    export AWS_SECRET_ACCESS_KEY="$AWS_SECRET_ACCESS_KEY"
    export AWS_SESSION_TOKEN="$AWS_SESSION_TOKEN"
    export AWS_REGION="${AWS_REGION:-us-east-1}"
else
    echo "📋 Using credentials from AWS CLI configuration"
    # Fall back to AWS CLI configuration for static credentials
    export AWS_ACCESS_KEY_ID=$(aws configure get aws_access_key_id)
    export AWS_SECRET_ACCESS_KEY=$(aws configure get aws_secret_access_key)
    export AWS_REGION=$(aws configure get region || echo "us-east-1")
    
    # Optional: Set session token if using temporary credentials
    if aws configure get aws_session_token &> /dev/null; then
        export AWS_SESSION_TOKEN=$(aws configure get aws_session_token)
    fi
fi

# Set S3 bucket name (you can change this to your actual bucket)
export AWS_S3_BUCKET_NAME="mychat-app-images-niyvopib"
# Force us-east-1 region for the existing bucket (LocationConstraint: null = us-east-1)
export AWS_S3_REGION="us-east-1"
# Also export in the format expected by .NET configuration
export AWS__S3__BucketName="$AWS_S3_BUCKET_NAME"
export AWS__S3__Region="$AWS_S3_REGION"

echo "🪣 Using S3 bucket: $AWS_S3_BUCKET_NAME"
echo "🌍 Using AWS region: $AWS_REGION (your default)"
echo "🪣 Using S3 region: $AWS_S3_REGION (for bucket)"
echo "🔧 Backend will use: AWS__S3__BucketName=$AWS__S3__BucketName"

# Check if bucket exists, create if it doesn't
echo "🔍 Checking if S3 bucket exists..."
if aws s3 ls "s3://$AWS_S3_BUCKET_NAME" 2>&1 | grep -q 'NoSuchBucket'; then
    echo "📦 Creating S3 bucket: $AWS_S3_BUCKET_NAME"
    aws s3 mb "s3://$AWS_S3_BUCKET_NAME" --region "$AWS_REGION"
    
    # Set bucket policy for public read access to images
    echo "🔒 Setting bucket policy for public read access..."
    cat > /tmp/bucket-policy.json << EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::$AWS_S3_BUCKET_NAME/*"
        }
    ]
}
EOF
    aws s3api put-bucket-policy --bucket "$AWS_S3_BUCKET_NAME" --policy file:///tmp/bucket-policy.json
    rm /tmp/bucket-policy.json
else
    echo "✅ S3 bucket already exists: $AWS_S3_BUCKET_NAME"
fi

echo ""
echo "🚀 Starting Docker Compose with AWS credentials..."
echo "Environment variables set:"
echo "  AWS_ACCESS_KEY_ID: ${AWS_ACCESS_KEY_ID:0:10}..."
echo "  AWS_SECRET_ACCESS_KEY: ${AWS_SECRET_ACCESS_KEY:0:10}..."
if [[ -n "$AWS_SESSION_TOKEN" ]]; then
    echo "  AWS_SESSION_TOKEN: ${AWS_SESSION_TOKEN:0:10}... (temporary credentials)"
fi
echo "  AWS_REGION: $AWS_REGION"
echo "  AWS_S3_BUCKET_NAME: $AWS_S3_BUCKET_NAME"
echo "  AWS__S3__BucketName: $AWS__S3__BucketName"
echo "  AWS__S3__Region: $AWS__S3__Region (us-east-1 for existing bucket)"
echo ""

# Start Docker Compose
docker-compose up --build
