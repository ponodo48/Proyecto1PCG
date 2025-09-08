using UnityEngine;
using UnityEngine.Windows;
using System.Collections.Generic;

[System.Serializable]
public class Rule
{
    public string predecessor;
    public string[] successors = { }; 
    public float[] probabilities;
}
public class L_System : MonoBehaviour
{

    //agregar simbolos, rotacion 3d
    public string[] alphabet = { "f", "F", "[", "]", "+", "-", "&", "^", "/", "\\", "|" };
    public string axiom;
    public Rule[] Rules;
    public GameObject model;
    public int turning_angle = 45;
    public int length;
    public int depth = 3;
    public float thickness = 1.0f; 
    public float Turning_Angle { get => turning_angle; set => turning_angle = (int)Mathf.Clamp(value, 15, 60); }
    public float Length { get => length; set => length = (int)value; }
    public float Depth { get => depth; set=> depth = (int)Mathf.Clamp(value,1,8); }
    public float Thickness { get => thickness; set => thickness = Mathf.Clamp(value, 0.3f, 3.0f); }
    public string Axiom { get => axiom; set => axiom = value.ToString(); }
    
    public void Expand(int depth)
    {
        if (depth == 0)
        {
            return;
        }
        else
        {
            var axioma = "";
            foreach (char c in axiom)
            {
                bool replaced = false;
                foreach (Rule rule in Rules)
                {
                    if (c.ToString() == rule.predecessor)
                    {
                        // Selección estocástica
                        float r = Random.value;
                        float acumulado = 0f;
                        for (int i = 0; i < rule.successors.Length; i++)
                        {
                            acumulado += rule.probabilities[i];
                            if (r <= acumulado)
                            {
                                axioma += rule.successors[i];
                                break;
                            }
                        }
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                {
                    axioma += c; // si no hay regla, dejarlo igual
                }
            }
            axiom = axioma;
            depth --;
            Expand(depth);
        }
    }
    public void Interpret(string expression)
    {
        Stack<(Vector3, Quaternion, float)> stack = new Stack<(Vector3, Quaternion, float)>();

        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        float currentThickness = thickness;

        foreach (char c in expression)
        {
            switch (c)
            {
                case 'F':
                    Vector3 start = position;
                    Vector3 end = position + rotation * Vector3.up * length;

                    GameObject branch = Instantiate(model, gameObject.transform);
                    branch.transform.position = (start + end) / 2f;
                    branch.transform.up = (end - start).normalized;
                    branch.transform.localScale = new Vector3(currentThickness, (end - start).magnitude / 2f, currentThickness);

                    position = end;
                    break;

                case '+': rotation *= Quaternion.Euler(0, 0, turning_angle); break;
                case '-': rotation *= Quaternion.Euler(0, 0, -turning_angle); break;
                case '&': rotation *= Quaternion.Euler(turning_angle, 0, 0); break;
                case '^': rotation *= Quaternion.Euler(-turning_angle, 0, 0); break;
                case '\\': rotation *= Quaternion.Euler(0, turning_angle, 0); break;
                case '/': rotation *= Quaternion.Euler(0, -turning_angle, 0); break;
                case '|': rotation *= Quaternion.Euler(0, 180, 0); break;

                case '[':
                    stack.Push((position, rotation, currentThickness));
                    currentThickness *= 0.7f; // 🔥 afina ramas
                    break;

                case ']':
                    (position, rotation, currentThickness) = stack.Pop();
                    break;
            }
        }
    }


    void Start()
    {
        Expand(depth);
        Interpret(axiom);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void Generate_L_System()
    {
        ClearChildren();
        Expand(depth);
        Interpret(axiom);
    }
}
