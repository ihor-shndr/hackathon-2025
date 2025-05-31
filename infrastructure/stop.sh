#!/bin/bash

echo "🛑 Stopping MyChat Application Stack..."
echo ""

# Stop all services
docker-compose down

echo ""
echo "✅ Application stack stopped." 