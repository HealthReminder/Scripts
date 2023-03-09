using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicLambdaExpressions : MonoBehaviour
{
    //Lambda functions are anonymous functions
    //You can create non-anonymous functions though
    //Anonymous functions work like this:
    //(input-parameters) => expression
    //or
    //(input-parameters) => {<sequence-of-statements>}
    //It can be used as events or delegates

    //An Action is a custom C# delegate
    Action delegate_example;

    private void Start()
    {
        PrintMessage("Called non-anonymous Lambda function");
        //Could be used anonymously in functions like
        //AddListener, which can only call functions that return void, because they are delegates
        //Or lambda functions
        //blabla.AddListener(() => PrintMessage("OI"));
        //Delegates work as follows:
        delegate_example += () => PrintMessage("Called anonymous Lambda function added to delegate");
        delegate_example();
        //The following code does not work because delegates can only call functions that return void
        //delegate_example += PrintMessage("Hi");
    }

    /// EXAMPLE OF NON-ANONYMOUS LAMBDA FUNCTION
    public void PrintMessage(string message) => Debug.Log("Message:" + message);
}
