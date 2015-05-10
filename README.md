# SuperPOCO

Store Plain Old CLR objects in Azure Table Storage with no dependency on the object model.

## Installation

You should be able to add this as a nuget reference, simply type

```
Install-Package SuperPoco
```

[nuget.com/superpoco](https://www.nuget.org/packages/SuperPoco/)

### Example

Below is a simple User class.
Traditionally you have to implement `ITableEntity` or `TableEntity` to store a strongly typed class in Azure Table Storage.
But with SuperPoco you don't!

```csharp

/// Sample User .NET object ... notice no table storage dependencies
public class MyUser{  
   public string Id {get; set;}
   public string Name {get;set;}
   public DateTime CreatedAt {get;set;}
   public string Email {get;set;}
}
```

To store the object, you can simple to convert the regular POCO user object into a table entity using
`EntityConverter.ConvertToDynamicTableEnity()`

```csharp
public static class UserExtensions
{
  public static void InsertUser(this MyUser user, CloudTable table){
    
    // convert user to table entity
    var entity = EntityConverter.ConvertToDynamicTableEntity(user, 
       u.Id  // choose a partitionKey
       u.Email // choose a rowKey
    );
    
    // create table insert operation
    var operation =  TableOperation.Insert(customer1);
    
    // Execute the insert operation.
    table.Execute(insertOperation);
  }
}

```

Notice how we selected the row and partition key from user object as the `Id` and `Email`.

To retreive the same object

```csharp
public static class UserExtensions
{
  public static User LookUpUser(CloudTable table, string userId, string email){
    
    // create table insert operation
    var operation =  TableOperation.Retrieve<DynamicTableEntity>(customer1);
    
    // Execute the insert operation.
    var result = table.Execute(lookupOperation);
    
    // convert user to table entity
    return EntityConverter.ConvertTo<MyUser>( (DynamicTableEntity)execute.Result);
  }
}
```

##### Complex Values

There is no support for complex values or arrays other than for binary `byte[]`

##### Ignoring Properties

You can easily ignore a value using the [JsonIgnore] type annotation .. allowing you to leverage the same object that you would use for json serialization

### License

MIT License see above
