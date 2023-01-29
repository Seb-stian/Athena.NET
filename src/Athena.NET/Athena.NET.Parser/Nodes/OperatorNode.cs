﻿using Athena.NET.Athena.NET.Lexer.Structures;
using Athena.NET.Athena.NET.Parser.Interfaces;
using Athena.NET.Athena.NET.Parser.Nodes.DataNodes;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Athena.NET.Athena.NET.Parser.Nodes
{
    internal abstract class OperatorNode : IEvaluationNode
    {
        public abstract int OperatorWeight { get; }

        public abstract TokenIndentificator NodeToken { get; }

        public ChildrenNodes ChildNodes { get; private set; }
        public int ChildNodesCount { get; } = 0;

        public OperatorNode(ReadOnlyMemory<Token> tokens)
        {
        }

        //TODO: Implement proper separation for
        //operators, that will support proper
        //preferences to braces
        public ChildrenNodes SepareteNodes(ReadOnlyMemory<Token> tokens, int nodeIndex)
        {
            return ChildrenNodes.BlankNodes;
        }

        public void Evaluate() 
        {
            var evaluatedLeftNode = GetEvaluatedNode(ChildNodes.LeftNode);
            var evaluatedRightNode = GetEvaluatedNode(ChildNodes.RightNode);

            ChildNodes = new(evaluatedLeftNode, evaluatedRightNode);
        }

        internal abstract int CalculateData(int firstData, int secondData);

        //TODO: Move this method into some
        //sort of parser helper class
        public static int IndexOfToken(ReadOnlyMemory<Token> tokens, TokenIndentificator token)
        {
            var tokensSpan = tokens.Span;
            int tokensLength = tokensSpan.Length;

            for (int i = 0; i < tokensLength; i++)
            {
                if (tokensSpan[i].TokenId == token)
                    return i;
            }
            return -1;
        }

        private INode GetEvaluatedNode(INode node) 
        {
            if(TryGetEvaluateNode(out IEvaluationNode currentNode, node))
            {
                currentNode.Evaluate();
                if (node.ChildNodes.LeftNode is DataNode<int> leftData &&
                    node.ChildNodes.RightNode is DataNode<int> rightData) 
                {
                    var returnData = CalculateData(leftData.NodeData, rightData.NodeData);
                    return new DataNode<int>(TokenIndentificator.Int, returnData);
                }
            }
            return node;
        }

        private bool TryGetEvaluateNode([NotNullWhen(true)]out IEvaluationNode evaluationNode, INode node)
        {
            bool isEvaluate = node.GetType().IsDefined(typeof(IEvaluationNode));
            evaluationNode = isEvaluate ? (IEvaluationNode)node : null!;
            return isEvaluate;
        }
    }
}
