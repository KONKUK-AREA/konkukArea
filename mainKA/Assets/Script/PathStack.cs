using System.Collections;
using System.Collections.Generic;

public class PathStack{
    LinkedListNode top;
    public PathStack()
    {
        top = null;
    }
    public void Push(string data)
    {
        LinkedListNode newNode = new LinkedListNode(data);
        if(top == null)
        {
            top = newNode;
            return;
        }
        newNode.next = top;
        top = newNode;
    }
    public string Pop()
    {
        string peek = null;
        if(top!= null)
        {
            peek = top.data;
            top = top.next;
        }
        return peek;
    }
    public bool isEmpty()
    {
        return top == null;
    }
    public int Remove(string data)
    {
        LinkedListNode preNode;
        LinkedListNode newNode;
        preNode = top;
        if (preNode.data.Equals(data))
        {
            top = preNode.next;
            return 1;
        }
        while ((newNode = preNode.next) != null)
        {
            if (newNode.data.Equals(data))
            {
                preNode.next = newNode.next;
                return 1;
            }
            preNode = newNode;
        }
        return -1;
    }
    public string getData(int index)
    {
        LinkedListNode Node = top;
        for(int i = 0; i < index; i++)
        {
            Node = Node.next;
        }
        return Node.data;
    }
    public int Length()
    {
        int length = 0;
        LinkedListNode node = top;
        while (node != null)
        {
            node = node.next;
            length++;
        }
        return length;
    }
}
public class LinkedListNode
{
    public string data;
    public LinkedListNode next;
    public LinkedListNode(string data)
    {
        this.data = data;
        this.next = null;
    }
}