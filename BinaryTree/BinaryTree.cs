using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BinaryTree
{
    public class BinaryTree<T> : IEnumerable<T>
    {
        class Node<TValue>
        {
            public TValue Value { get; set; }
            public Node<TValue> Left { get; set; }
            public Node<TValue> Right { get; set; }

            public Node(TValue value)
            {
                Value = value;
            }
        }

        private Node<T> root;

        public delegate void TreeEventHandler(object sender, TreeEventArgs<T> args);

        public event TreeEventHandler ElementAdded;

        public event TreeEventHandler ElementRemoved;

        public int Count { get; private set; }

        readonly IComparer<T> comparer;

        public BinaryTree()
        {
            if (typeof(T).GetInterface("IComparable") == null && typeof(T).Name != "ComparableClass")
            {
                string message = "The type must implement IComparable";
                throw new ArgumentException(message);
            }
            comparer = Comparer<T>.Default;
        }

        public BinaryTree(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        public void Add(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            Node<T> before = null;
            var after = this.root;

            while (after != null)
            {
                before = after;
                var comparedItems = comparer.Compare(item, after.Value);
                if (comparedItems < 0) after = after.Left;
                else after = after.Right;
            }

            var newNode = new Node<T>(item);
            ++Count;
            if (root is null) root = newNode;
            else
            {
                int comparedItems = comparer.Compare(item, before.Value);
                if (comparedItems < 0) before.Left = newNode;
                else if (comparedItems > 0) before.Right = newNode;

            }

            ElementAdded?.Invoke(this, new TreeEventArgs<T>(item, "Added node!"));
        }

        public bool Remove(T item)
        {
            if (item == null) return false;
            if (root == null) return false;

            Node<T> current = root, parent = current;

            int result;
            do
            {
                result = comparer.Compare(item, current.Value);
                if (result < 0) { parent = current; current = current.Left; }
                else if (result > 0) { parent = current; current = current.Right; }
                if (current == null) return false;
            }
            while (result != 0);

            if (current.Right == null)
            {
                if (current == root) root = current.Left;
                else
                {
                    result = comparer.Compare(current.Value, parent.Value);
                    if (result < 0) parent.Left = current.Left;
                    else parent.Right = current.Left;

                }
            }
            else if (current.Right.Left == null)
            {
                current.Right.Left = current.Left;
                if (current == root) root = current.Right;
                else
                {
                    result = comparer.Compare(current.Value, parent.Value);
                    if (result < 0) parent.Left = current.Right;
                    else parent.Right = current.Right;

                }
            }
            else
            {
                Node<T> min = current.Right.Left, prev = current.Right;
                while (min.Left != null)
                {
                    prev = min;
                    min = min.Left;
                }
                prev.Left = min.Right;
                min.Left = current.Left;
                min.Right = current.Right;
                if (current == root) root = min;
                else
                {
                    result = comparer.Compare(current.Value, parent.Value);
                    if (result < 0) parent.Left = min;
                    else parent.Right = min;

                }
            }

            ElementRemoved?.Invoke(this, new TreeEventArgs<T>(item, "Remove node!"));
            --Count;
            return true;

        }

        public T TreeMax()
        {
            if (root == null) throw new InvalidOperationException("Tree is null");
            var current = root;
            while (current.Right != null)
                current = current.Right;
            return current.Value;
        }

        public T TreeMin()
        {
            if (root == null) throw new InvalidOperationException("Tree is null");
            var current = root;
            while (current.Left != null)
                current = current.Left;
            return current.Value;
        }

        public bool Contains(T data)
        {
            var current = root;
            while (current != null)
            {
                var result = comparer.Compare(data, current.Value);
                if (result == 0) return true;
                if (result < 0) current = current.Left;
                else current = current.Right;
            }
            return false;
        }

        public IEnumerable<T> Traverse(TraverseType traverseType)
        {
            if (traverseType == TraverseType.PreOrder)
            {
                if (root == null) yield break;

                var stack = new Stack<Node<T>>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    yield return node.Value;
                    if (node.Right != null) stack.Push(node.Right);
                    if (node.Left != null) stack.Push(node.Left);
                }
            }
            else if (traverseType == TraverseType.InOrder)
            {
                if (root == null) yield break;

                var stack = new Stack<Node<T>>();
                var node = root;

                while (stack.Count > 0 || node != null)
                {
                    if (node == null)
                    {
                        node = stack.Pop();
                        yield return node.Value;
                        node = node.Right;
                    }
                    else
                    {
                        stack.Push(node);
                        node = node.Left;
                    }
                }
            }
            else
            {
                if (root == null) yield break;

                var stack = new Stack<Node<T>>();
                var node = root;

                while (stack.Count > 0 || node != null)
                {
                    if (node == null)
                    {
                        node = stack.Pop();
                        if (stack.Count > 0 && node.Right == stack.Peek())
                        {
                            stack.Pop();
                            stack.Push(node);
                            node = node.Right;
                        }
                        else { yield return node.Value; node = null; }
                    }
                    else
                    {
                        if (node.Right != null) stack.Push(node.Right);
                        stack.Push(node);
                        node = node.Left;
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Traverse(TraverseType.InOrder).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}