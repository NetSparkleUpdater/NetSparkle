# Sample Application Release Notes

## 104.1

* Fixed bug where ExecuteAsync sometimes doesn't send data

## 104.0

* Fixed Windows Phone and Silverlight to use culture when calling Convert.ChangeType() (thanks trydis)
* Added support for non-standard HTTP methods (thanks jhoerr)  
  New API methods include:
  * `IRestClient.ExecuteAsyncGet()`
  * `IRestClient.ExecuteAsyncPost()`
  * `IRestClient.ExecuteAsyncGet<T>()`
  * `IRestClient.ExecuteAsyncPost<T>()`
  
  See [groups discussion](https://groups.google.com/forum/?fromgroups=#!topic/restsharp/FCLGE5By7AU) for more info

* Resolved an xAuth support issue in the OAuth1Authenticator (thanks artema)
* Change AddDefaultParameter methods to be extension methods (thanks haacked)  
  Added `RestClientExtensions.AddDefaultParameter()` with 4 overloads. See pull request [#311](https://github.com/restsharp/RestSharp/pull/311) for more info

* Adding support for deserializing enums from integer representations (thanks dontjee)