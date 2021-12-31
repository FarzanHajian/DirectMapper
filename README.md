# DirectMapper
DirectMapper is a simple and light object mapper for .NET. Since simplicity is a purpose, it only maps properties with same names. Here, there is no way to defined complex rules to map everything to everything.

DirectMapper also meant to be light-weight thus a great choice for Blazor WASM applications.

## Installation
For now, there is no Nuget package (I promise to provide one soon). Just copy the DirectMapper.cs file from DirectMapper project into to your own project.  

## Usage
Let's assume we have a product model class and a product view model class.

```csharp
public class Product
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public DateTime IntroductionDate { get; set; }

    public int InStockQty { get; set; }

    public string FullName=> $"{Code} - {Name}";
    
    public bool Available => InStockQty>0;        
}

public class ProductVM
{
    public string Name { get; set; }

    public string FullName { get; set; }

    public int InStockQty { get; set; }

    public string IntroductionDate { get; set; }
}
```

We can use DirectMapper make clones (shallow copies) or view model objects from our product objects.

```csharp
using DirectMapper;

.
.
.

// The original model object
var tea = new Product{
    Id=1, Code="001",
    Name = "Tea", InStockQty=20,
    IntroductionDate=new DateTime(2018,10,3)
};

// Creating a clone object by shallow copying properties.
var clone = tea.DirectMap<Product, Product>();            

// Creating a view model instance.
/* NOTE: Only Name, FullName and InStockQty properties
 * are copied but not IntroductionDate. Read the
 * explanation below. */
var vm = tea.DirectMap<Product, ProductVM>();
```

For a property to be copied when source and destination data types differ (mapping model to view model in our example), following requirements must be satisfied:
- A property with the exact same name as the source one must be available in the destination.
- The destionation property must have a setter.
- The destination property must have the exact same data type as the source one.

The first and the second requirements cannot be ignored. The third one on the other hand, can be compromised by defining rules (either global rules or type-specific ones).

### Rules
As discussed ealier, by default data types of source and destination properties must be the same. If there is a need for conversion, you will have to define rules.

There are two types of rules. **Entity-specific** and **global** and (of course) entity-specific rules have higher priority than global rules.

**Keep in mind that rules must be defined before any mapping operations.**

**Rules are applied only when mapping between two different data types.**

To following code snippet defines a rule to convert *IntroductionDate* property from *Product* to *ProductVM*.

```csharp
using DirectMapper;

// Creating a shorter alias to make eveything easier 
using DMapper = DirectMapper.DirectMapper;
.
.
.
DMapper.BuildMapper<Product, ProductVM>()
    .WithRule<DateTime, string>(
        "IntroductionDate",
        src=> src.ToString("ddd, dd MMM yyyy hh:mm:ss")
    )
    .Build();

// Now IntroductionDate in in vm has data 
var vm = tea.DirectMap<Product, ProductVM>();

```
 
Know let's assume you want to apply the above rule on all mappings (not only on mapping from Product to ProductVM). Here global rules can help.

```csharp
using DirectMapper;

// Creating a shorter alias to make eveything easier 
using DMapper = DirectMapper.DirectMapper;
.
.
.
DMapper.BuildGlobalRules()
    .WithRule<DateTime, string>(src=> src.ToString("ddd, dd MMM yyyy hh:mm:ss"))
    .Build();

// Now IntroductionDate in in vm has data 
var vm = tea.DirectMap<Product, ProductVM>();

/* And other any mapping that involve converting a
   DateTime to a String, the rule is utilized. */
```
Global ToString rule is a special global to that is applied when a value must be mapped to a string data type and there is no specific and global rule found.

```csharp
DMapper.BuildGlobalRules()
    .WithGlobalToString()
    .Build();
```

Note that the API follows the fluent syntax and you can chain rule definitions.

```csharp
using DirectMapper;
using DMapper = DirectMapper.DirectMapper;

DMapper.BuildGlobalRules()
    .WithRule<DateTime, string>(src=> src.ToString("ddd, dd MMM yyyy hh:mm:ss"))
    .WithRule<string, DateTime>(src=> DateTime.Parse(src))
    .WithGlobalToString()
    .Build();

DMapper.BuildMapper<Product, ProductVM>()
    .WithRule<DateTime, string>("IntroductionDate", src=> src.ToString("yyyy/MM/dd"))
     // It may by useless to define a rule when source and destination have the same data type but it is possible :)
    .WithRule<string, string>("Name", src=>src.ToUpper())
    .Build();

DMapper.BuildMapper<Product, ProductVM2>()
    .   // rule 1
    .   // rule 2
    .   // rule n
    .Build();

.
.
.
```
