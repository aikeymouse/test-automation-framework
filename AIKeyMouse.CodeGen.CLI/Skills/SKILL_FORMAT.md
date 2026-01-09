# Skill File Format Guide

This guide explains how to create `.skill.md` files for the AIKeyMouse Code Generator CLI.

## Overview

Skills are defined in Markdown files with YAML frontmatter. This format is:
- ✅ Human-readable and easy to maintain
- ✅ Supports syntax highlighting in editors
- ✅ No escaped newlines or special characters
- ✅ Git-friendly with clear diffs

## File Structure

A skill file consists of three main parts:

```
---
YAML Frontmatter (metadata and configuration)
---

# System Prompt
The LLM's role and instructions

# Prompt Template
Liquid template for generating prompts

# Examples
Example inputs, outputs, and explanations
```

## YAML Frontmatter

The frontmatter contains metadata and configuration using YAML syntax:

```yaml
---
id: unique-skill-identifier
name: Human-Readable Skill Name
description: Brief description of what this skill generates
category: skill-category  # page-object, step-definition, etc.
platform: web  # web, mobile, desktop, or all

llmParams:
  temperature: 0.3  # 0.0-1.0, lower = more focused
  maxTokens: 2000   # Max tokens to generate

validation:
  requiredUsings:
    - Namespace.One
    - Namespace.Two
  requiredBaseClass: BaseClassName  # Optional
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
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | ✅ Yes | Unique identifier for the skill |
| `name` | string | ✅ Yes | Display name shown to users |
| `description` | string | ✅ Yes | What this skill generates |
| `category` | string | ✅ Yes | Skill category (page-object, step-definition, etc.) |
| `platform` | string | ✅ Yes | Target platform: web, mobile, desktop, all |
| `llmParams.temperature` | float | No | Creativity level (0.0-1.0, default: 0.3) |
| `llmParams.maxTokens` | int | No | Maximum tokens to generate (default: 2000) |
| `validation.requiredUsings` | list | No | Required C# using statements |
| `validation.requiredBaseClass` | string | No | Required base class name |
| `validation.validateSyntax` | bool | No | Validate C# syntax (default: true) |
| `validation.autoFormat` | bool | No | Auto-format generated code (default: true) |
| `output.defaultPath` | string | No | Default output directory |
| `output.fileNamePattern` | string | No | File name pattern (use {name} placeholder) |
| `output.defaultNamespace` | string | No | Default C# namespace |
| `output.codeExtraction.extractCodeBlocks` | bool | No | Extract code from markdown blocks |
| `output.codeExtraction.language` | string | No | Expected language (csharp, python, etc.) |
| `output.codeExtraction.stripMarkdown` | bool | No | Remove markdown formatting |

## System Prompt Section

Define the LLM's role and general instructions:

```markdown
# System Prompt

You are an expert in [domain]. Generate clean, maintainable [language] code 
following [framework] best practices with proper [specific requirements].
```

**Guidelines:**
- Keep it concise (2-3 sentences)
- Define the LLM's expertise
- Specify code quality expectations
- Mention key patterns or practices

## Prompt Template Section

Use Liquid template syntax to create dynamic prompts:

```markdown
# Prompt Template

Generate a [type] class for {{ platform }} with the following details:

**Name:** {{ className }}
**Namespace:** {{ namespace | default: 'DefaultNamespace' }}
{% if description -%}
**Description:** {{ description }}
{% endif -%}

{% if elements -%}
**Elements:**
{% for element in elements -%}
- {{ element.name }} ({{ element.type }}): {{ element.locator }}
{% endfor -%}
{% endif -%}

**Requirements:**
- Requirement 1
- Requirement 2
- Follow naming convention: {{ className }}Suffix
```

### Liquid Syntax Reference

| Syntax | Purpose | Example |
|--------|---------|---------|
| `{{ variable }}` | Output variable | `{{ pageName }}` |
| `{{ var \| filter }}` | Apply filter | `{{ name \| default: 'DefaultValue' }}` |
| `{% if condition -%}` | Conditional | `{% if url -%}URL: {{ url }}{% endif -%}` |
| `{% for item in list -%}` | Loop | `{% for el in elements -%}{{ el.name }}{% endfor -%}` |
| `{%- ... -%}` | Strip whitespace | Removes newlines around block |

**Common filters:**
- `default: 'value'` - Provide default if variable is null/empty
- `upcase` / `downcase` - Change case
- `capitalize` - Capitalize first letter

## Examples Section

Provide concrete examples with input, output, and explanations:

```markdown
# Examples

## Example 1: Simple Case

### Input

```
[Plain text or structured format showing what the user provides]
```

### Output

```csharp
[Complete, compilable code that demonstrates the expected result]
```

### Explanation

Brief explanation of the example, highlighting key features or patterns.

## Example 2: Complex Case

### Input

```
[Another example input]
```

### Output

```csharp
[Another example output]
```

### Explanation

What makes this example different or important.
```

**Guidelines:**
- Include 1-3 examples
- Use `## Example N:` headers
- Always include Input, Output, and Explanation subsections
- Input can be plain text or structured format
- Output should be complete, compilable code
- Use proper code block language identifiers (```csharp, ```python, etc.)

## Complete Template

Use this template to create new skills:

```markdown
---
id: my-skill-id
name: My Skill Name
description: What this skill generates
category: category-name
platform: web
llmParams:
  temperature: 0.3
  maxTokens: 2000
validation:
  requiredUsings:
    - Required.Namespace
  validateSyntax: true
  autoFormat: true
output:
  defaultPath: OutputFolder
  fileNamePattern: '{name}ClassName.cs'
  defaultNamespace: My.Namespace
  codeExtraction:
    extractCodeBlocks: true
    language: csharp
    stripMarkdown: true
---

# System Prompt

You are an expert in [domain]. Generate clean, maintainable code following best practices.

# Prompt Template

Generate a class with the following details:

**Name:** {{ className }}
**Namespace:** {{ namespace | default: 'DefaultNamespace' }}

**Requirements:**
- Requirement 1
- Requirement 2

# Examples

## Example 1: Basic Example

### Input

```
Input description or sample data
```

### Output

```csharp
using System;

namespace MyNamespace
{
    public class GeneratedClass
    {
        // Generated code here
    }
}
```

### Explanation

Brief explanation of what this example demonstrates.
```

## Best Practices

### 1. Clear and Specific System Prompts
- Define expertise domain clearly
- Specify coding standards
- Mention framework-specific patterns

### 2. Flexible Prompt Templates
- Use Liquid variables for user inputs
- Provide sensible defaults with `| default: 'value'`
- Use conditionals to handle optional parameters
- Keep formatting clean and readable

### 3. Quality Examples
- Show realistic, complete code
- Cover common use cases
- Include edge cases if relevant
- Ensure examples are compilable
- Add explanations for complex patterns

### 4. Proper Configuration
- Set appropriate temperature (0.1-0.4 for code generation)
- Configure maxTokens based on expected output size
- Specify all required namespaces
- Use validation to ensure quality output

## File Naming Convention

- Use lowercase with hyphens: `my-skill.skill.md`
- Include category prefix: `page-object-web.skill.md`
- Place in appropriate category folder:
  - `Skills/PageObjects/` for page object skills
  - `Skills/StepDefinitions/` for step definition skills
  - `Skills/[Category]/` for custom categories

## Testing Your Skill

1. **Create the skill file** in the appropriate directory
2. **Build the project** to copy the file to output:
   ```bash
   dotnet build
   ```
3. **Test with a simple command**:
   ```bash
   dotnet run -- page --name TestPage --output /tmp/test
   ```
4. **Verify the output**:
   - Check file was created
   - Verify syntax is valid
   - Ensure code follows requirements
   - Test with edge cases

## Troubleshooting

### Skill Not Loading
- Check file extension is `.skill.md`
- Verify YAML frontmatter is valid (use online YAML validator)
- Ensure required fields are present (id, name, description, category, platform)
- Check file is in correct category folder

### YAML Parse Errors
- Verify proper indentation (use spaces, not tabs)
- Ensure strings with special characters are quoted
- Check list items start with `-` and have proper indentation
- Use online YAML validator to check syntax

### Template Not Working
- Verify Liquid syntax is correct
- Check variable names match what's passed from commands
- Test conditionals with real data
- Ensure `{%- -%}` is used correctly to strip whitespace

### Code Quality Issues
- Increase temperature slightly if output is too rigid
- Decrease temperature if output is inconsistent
- Add more specific requirements in prompt template
- Include better examples demonstrating desired patterns
- Configure validation settings appropriately

## Advanced Features

### Skill Inheritance (Future)
Skills can inherit from other skills using `extends`:

```yaml
---
id: my-custom-skill
extends: base-page-object
# ... override specific properties
---
```

### Skill Imports (Future)
Import examples and metadata from other skills:

```yaml
---
id: my-skill
imports:
  - common-patterns
  - validation-examples
---
```

## Additional Resources

- **Liquid Documentation**: https://shopify.github.io/liquid/
- **YAML Specification**: https://yaml.org/spec/
- **Example Skills**: See `Skills/PageObjects/page-object-web.skill.md` and `Skills/StepDefinitions/step-definition-web.skill.md`
