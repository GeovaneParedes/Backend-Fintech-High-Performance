#!/usr/bin/env bash

# 🔥 Chaos Monkey Simulator para Kubernetes / AWS EKS
# Simula a injeção de caos em tempo real para testar o auto-recovery do Kubernetes e o Polly Retry/Circuit Breaker

NAMESPACE="fintech-production"
INTERVAL_SECONDS=10

echo "=================================================================="
echo "🔥 CHAOS MONKEY INITIALIZED IN KUBERNETES CLUSTER [AWS EKS]"
echo "=================================================================="
echo "🎯 Target Namespace: ${NAMESPACE}"
echo "⏱️ Interval: ${INTERVAL_SECONDS} seconds"
echo "=================================================================="

while true; do
    echo ""
    echo "[Chaos Agent] Selecionando um Pod do Fintech Core aleatoriamente..."
    
    # Seleciona um Pod aleatório do Deployment
    POD_NAME=$(kubectl get pods -n ${NAMESPACE} -l app=fintech-core-api -o jsonpath='{.items[*].metadata.name}' | tr ' ' '\n' | shuf -n 1)
    
    if [ -n "$POD_NAME" ]; then
        echo "💥 KILLING POD: ${POD_NAME}"
        kubectl delete pod ${POD_NAME} -n ${NAMESPACE} --grace-period=0 --force
        echo "✅ Pod destruído! O Kubernetes HPA/Deployment re-criará o Pod imediatamente."
    else
        echo "⚠️ Nenhum Pod encontrado no namespace ${NAMESPACE}."
    fi

    sleep ${INTERVAL_SECONDS}
done
