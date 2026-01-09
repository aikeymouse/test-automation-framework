---
id: template-skill
name: Template Skill Name
description: Brief description of what this skill generates
category: category-name
platform: web
llmParams:
  temperature: 0.3
  maxTokens: 2000
validation:
  requiredUsings:
    - Required.Namespace.One
    - Required.Namespace.Two
  requiredBaseClass: OptionalBaseClass
  validateSyntax: true
  autoFormat: true
output:
  defaultPath: OutputFolder
  fileNamePattern: '{name}ClassName.cs'
  defaultNamespace: Default.Namespace
  codeExtraction:
    extractCodeBlocks: true
    language: csharp
    stripMarkdown: true
---

# System Prompt

You are an expert in [domain/technology]. Generate clean, maintainable [language] code following [framework/pattern] best practices with proper [specific requirements like encapsulation, documentation, error handling].

# Prompt Template

Generate a [type of artifact] for {{ platform }} with the following details:

**Name:** {{ className }}
**Namespace:** {{ namespace | default: 'DefaultNamespace' }}
{% if description -%}
**Description:** {{ description }}
{% endif -%}

{% if customProperty -%}
**Custom Property:** {{ customProperty }}
{% endif -%}

{% if listItems -%}
**List Items:**
{% for item in listItems -%}
- {{ item.name }}: {{ item.value }}
{% endfor -%}
{% endif -%}

**Requirements:**
- Requirement 1: [specific requirement]
- Requirement 2: [specific requirement]
- Requirement 3: [specific requirement]
- Follow naming convention: {{ className }}[Suffix]

# Examples

## Example 1: Basic Example

### Input

```
Simple input showing the minimum required information.
Key: Value
Another Key: Another Value
```

### Output

```csharp
using Required.Namespace.One;
using Required.Namespace.Two;

namespace Default.Namespace
{
    /// <summary>
    /// Brief description of the generated class
    /// </summary>
    public class GeneratedClass : OptionalBaseClass
    {
        // Properties
        public string PropertyOne { get; set; }
        
        // Constructor
        public GeneratedClass()
        {
            // Initialization logic
        }
        
        // Methods
        /// <summary>
        /// Method description
        /// </summary>
        public void MethodOne()
        {
            // Implementation
        }
    }
}
```

### Explanation

This example demonstrates the basic structure with minimal requirements. Shows standard patterns like XML documentation, property definitions, and method structure.

## Example 2: Complex Example

### Input

```
More complex input with additional properties and requirements.
Name: ComplexExample
Property1: Value1
Property2: Value2
List:
  - Item1
  - Item2
  - Item3
```

### Output

```csharp
using Required.Namespace.One;
using Required.Namespace.Two;
using System.Collections.Generic;

namespace Default.Namespace
{
    /// <summary>
    /// Complex example demonstrating advanced patterns
    /// </summary>
    public class ComplexExampleClass : OptionalBaseClass
    {
        private readonly List<string> _items;
        
        public string Property1 { get; set; }
        public string Property2 { get; set; }
        
        public ComplexExampleClass()
        {
            _items = new List<string> { "Item1", "Item2", "Item3" };
        }
        
        /// <summary>
        /// Process items with custom logic
        /// </summary>
        public void ProcessItems()
        {
            foreach (var item in _items)
            {
                // Process each item
            }
        }
        
        /// <summary>
        /// Advanced method with parameters
        /// </summary>
        /// <param name="input">Input parameter</param>
        /// <returns>Processed result</returns>
        public string AdvancedMethod(string input)
        {
            // Implementation with return value
            return $"Processed: {input}";
        }
    }
}
```

### Explanation

This example shows more complex patterns including collections, private fields, methods with parameters and return values, and more sophisticated logic. Demonstrates how to handle lists and implement business logic.
