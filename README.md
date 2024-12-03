# DirectMapper
DirectMapper is a simple and light object mapper for .NET. Since simplicity is a purpose, it only maps properties with same names. Here, there is no way to defined complex rules to map everything to everything.

DirectMapper is also meant to be lightweight and thus a great choice for Blazor WASM applications.

## Installation
The preferred method for installing the library is to copy the DirectMapper.cs file into the destination project, but for those who prefer installing using a package, the Classmate package is available on [NuGet](https://www.nuget.org/packages/FarzanHajian.DirectMapper/).

## Usage
Let’s assume we have a product model class and a product view model class.

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

We can use DirectMapper to make clones (shallow copies) of view model objects from our product objects.

```csharp
using FarzanHajian.DirectMapper;

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

For a property to be copied when source and destination data types differ (mapping model to view model in our example), the following requirements must be satisfied:
- A property with the same name as the source must be available in the destination.
- The destination property must have a setter.
- The destination property must have the same data type as the source one.

The first and the second requirements cannot be ignored. The third one, on the other hand, can be overridden by defining rules.

### Rules
As discussed earlier, by default, data types of source and destination properties must be the same. If there is a need for conversion, you will have to define rules.

There are two types of rules:
- **Type-specific** rules define how objects of a certain type must be mapped to objects of another type.
- **Global** rules are general rules that must be applied in the absence of suitable type-specific rules.

Here is a list of items to bear in mind when using rules:
- Type-specific rules have higher priority than global rules.
- Global rules must be defined before type-specific rules are defined.
- Rules are applied only when source and destination properties have different data types.

The following code snippet defines a rule to convert the *IntroductionDate* property from *Product* to *ProductVM*.

```csharp
using DirectMapper;

// Creating a shorter alias to make everything easier 
using DMapper = FarzanHajian.DirectMapper.DirectMapper;
.
.
.
DMapper.BuildMapper<Product, ProductVM>()
    .WithRule<DateTime, string>(
        "IntroductionDate",
        src=> src.ToString("ddd, dd MMM yyyy hh:mm:ss")
    )
    .Build();

// Now the following command can fill IntroductionDate in the view model with the proper data
var vm = tea.DirectMap<Product, ProductVM>();

```
 
Know let's assume you want to apply the above rule on all mappings (not only on mapping from Product to ProductVM). Here global rules can help.

```csharp
using FarzanHajian.DirectMapper;

// Creating a shorter alias to make everything easier 
using DMapper = FarzanHajian.DirectMapper.DirectMapper;
.
.
.
DMapper.BuildGlobalRules()
    .WithRule<DateTime, string>(src=> src.ToString("ddd, dd MMM yyyy hh:mm:ss"))
    .Build();

// Now the following command can fill IntroductionDate in the view model with the proper data
var vm = tea.DirectMap<Product, ProductVM>();

/* Keep in mind that this time we defined a global rule so the rule will be used for all mapping
   that require a DateTime to string conversion. */
.
.
.
```
The global **ToString** rule is a special global to that is applied when a value must be mapped to a string data type and there is no type-specific and global rule available.

```csharp
DMapper.BuildGlobalRules()
    .WithGlobalToString()
    .Build();
```

Note that the API follows the fluent syntax and you can chain rule definitions.

```csharp
using FarzanHajian.DirectMapper;
using DMapper = FarzanHajian.DirectMapper.DirectMapper;

DMapper.BuildGlobalRules()
    .WithRule<DateTime, string>(src=> src.ToString("ddd, dd MMM yyyy hh:mm:ss"))
    .WithRule<string, DateTime>(src=> DateTime.Parse(src))
    .WithGlobalToString()
    .Build();

DMapper.BuildMapper<Product, ProductVM>()
    .WithRule<DateTime, string>("IntroductionDate", src=> src.ToString("yyyy/MM/dd"))
    .WithRule<string, string>("Name", src=>src.ToUpper()) // It may be useful to define a rule when source and destination have the same data type
    .Build();

DMapper.BuildMapper<Product, ProductVM2>()
    .   // rule 1
    .   // rule 2
    .   // rule n
    .Build();.
.
.
.
```

Besides the *DirectMap* method, the *DirectMapper* class contains two more methods that come in handy when mapping different objects:
- *DirectMapRange<TSource, TDestination>* accepts a collection of objects and maps them to the destination data type altogether.
- *GetMapper<TSource, TDestination>* returns the exact function generated to map objects of type TSource to TDestination. Although typically there is no need to invoke it manually, it is there in case it is needed.