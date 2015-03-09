# MockFiles

Helper for C# testing. Create json mock data from your integration tests, and re-use them on the fly in your unit tests.

## Usage

In your integration tests, stub out your return data using *MockProvider.RegisterStub*. The class must implement an interface.


```c#
            var band = new Band(); // implements IBand
            var members = band.GetMembers();
            MockProvider.RegisterStub(band, new Func<List<Member>>(band.GetMembers), members);
            /// Creates file "IBand.GetMembers.json"
```

Once the json files exist in your test folder. Use them as much as you want in your unit tests by creating mocks using *MockProvider.GetMock<T>* where T is the Interface to be used.


```c#
            var mockBand = MockProvider.GetMock<IBand>();
            var stubMembers = mockBand.GetMembers();
```

Methods with parameters will create json files with the signature described in the filename.
eg. 
```
var members = band.GetMembersByStatus(true);
MockProvider.RegisterStub(band, new Func<bool, List<Member>>(band.GetMembersByStatus), members); 
// will create a json file named "IBand.GetMembersByStatus_Boolean.json"
