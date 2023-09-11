using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void TreeVisitor<T>(T cuurentNode, T childNode);
public class TreeNode<T>
{
    private T data;
    private LinkedList<TreeNode<T>> _children;
    public LinkedList<TreeNode<T>> children => _children;

    public TreeNode(T data)
    {
        this.data = data;
        _children = new LinkedList<TreeNode<T>>();
    }

    public void AddChild(TreeNode<T> node)
    {
        children.AddFirst(node);
    }

    public void Traverse(TreeVisitor<T> visitor)
    {
        foreach (var child in _children)
        {
            visitor(this.data, child.data);
            child.Traverse(visitor);
        }
    }
}
