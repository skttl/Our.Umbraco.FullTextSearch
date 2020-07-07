# Full Text Search
FullTextSearch works by extending the default `ExternalIndex` in Umbraco with its own fields for full text content. It renders a page by making a http request to the page, and indexes the content. If the server is unable to make the request, no content will be indexed.

The rendered content of nodes is cached in a seperate database table, so rendering can be skipped when reindexing Examine. FullTextSearch listens to events from the `ContentCacheRefresher`, and updates its own cache upon the `CacheUpdated` event.

The rendering is managed by a queue of tasks. You can always see the current queue on the FullTextSearch dashboard in the settings section of Umbraco. From here, you can also rebuild and reindex all nodes.

## Installation
Install using the nuget package, or from the package repository.
This package requires Umbraco 8.1 as a minimum version.

## Configuration
FullTextSearch needs to be enabled in your web.config by adding an application setting with the key `FullTextSearch.Enabled` and a value of True.

Here is an overview of the different config settings you can add.

`FullTextSearch.Enabled`
This enables the full text indexing. If the setting does not exists, or is set to false, full text indexing will not happen. The default value is `False`.

`FullTextSearch.DefaultTitleFieldName`
The name of the field containing the title of the page in the index. The default value is `nodeName`. You can also override this when searching.

`FullTextSearch.DisallowedContentTypeAliases`
Aliases of content types that should NOT be indexed by FullTextSearch. If you add an alias here, the nodes of that alias will not be rendered and indexed by FullTextSearch. By default nodes without templates are excluded, so you don't need to add them here.
Adding to this config on a site already indexed doesn't clean the index. You have to do this manually - but the search will exclude nodes of the disallowed types for you.

`FullTextSearch.DisallowedPropertyAliases`
You can use this setting to create your own "umbracoNaviHide" for FullTextSearch. Add a boolean property to your document type, and reference its alias in this setting. If the node has that property set to true, it will be excluded from full text indexing.
Adding to this config on a site already indexed doesn't clean the index. You have to do this manually - but the search will exclude nodes with the disallowed properties for you.

`FullTextSearch.FullTextFieldName`
The field name where FullTextSearch should store the rendered full text. The default of this is `FullTextSearch`.

`FullTextSearch.HttpTimeout`
This field contains the timeout (in seconds) for requests made when rendering content for full text indexing. The default value is `120` seconds. After that the request is aborted.

`FullTextSearch.FullTextPathFieldName`
FullTextSearch uses its own path field in examine, to be able to filter nodes by root node. You can configure the name of this here. The default value is `FullTextPath`.

`FullTextSearch.SearchActiveStringName`
When rendering, FullTextSearch adds this value as a querystring parameter, so you can use that to send different content to the indexer. The default value is `FullTextActive`

`FullTextSearch.SearchTitleBoost`
When searching, FullTextSearch boosts the title field by the value of this. The default value is `10.0`.

`FullTextSearch.XpathsToRemoveFromFullText`
When indexing rendered content, elements in the html output matching regexes in this field is removed. You can add more, and separate them with commas, like `//script,//head`. Defaults to empty.

#Searching
The preferred way of searching is by using the `ISearchService` interface. Below is the simplest possible example of a controller for a search node.
```
using System.Web.Mvc;
using Our.Umbraco.FullTextSearch.Interfaces;
using Our.Umbraco.FullTextSearch.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace MyProject.Controllers
{
    public class SearchController : RenderMvcController
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;

        }

        public override ActionResult Index(ContentModel model)
        {
            if (Request["q"] != null)
            {
                int.TryParse(Request["p"], out int currentPage);
                currentPage = currentPage < 1 ? 1 : currentPage;

                var search = new Search(Request["q"])
                    .SetCulture(model.Content.GetCultureFromDomains().ToLower());

                ViewBag.FullTextSearchResult = _searchService.Search(search, currentPage);
            }
            return base.Index(model);
        }
    }
}

```
In this example I add `ISearchService` as a dependencies for the controller. `ISearchService` performs the searching, and takes a `Search` object, which can be configured in numerous ways. The result is then added to the `ViewBag`. Notice that you need to set the culture of the search in order to get results from the right fields. Also notice the `Request["p"]` and `Request["q"]` variables used for paging and getting search terms.

My preferred way of searching looks like this:
```
using Our.Umbraco.FullTextSearch.Interfaces;

namespace Umbraco.Web.PublishedModels
{
    public partial class Search
    {
        public IFullTextSearchResult FullTextSearchResult { get; set; }
    }
}

```
```
using System.Web.Mvc;
using Our.Umbraco.FullTextSearch.Interfaces;
using Our.Umbraco.FullTextSearch.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace MyProject.Controllers
{
    public class SearchController : RenderMvcController
    {
        private readonly ISearchService _searchService;
        private readonly IConfig _config;

        public SearchController(ISearchService searchService, IConfig config)
        {
            _searchService = searchService;
            _config = config;
        }

        public override ActionResult Index(ContentModel model)
        {
            var searchModel = model.Content as Umbraco.Web.PublishedModels.Search;

            if (Request["q"] != null)
            {
                int.TryParse(Request["p"], out int currentPage);
                currentPage = currentPage < 1 ? 1 : currentPage;

                var search = new Search(Request["q"])
                    .EnableHighlighting()
                    .AddTitleProperty("metaTitle")
                    .AddTitleProperty("nodeName")
                    .AddSummaryProperty("metaDescription")
                    .AddSummaryProperty(_config.GetFullTextFieldName())
                    .SetSummaryLength(160)
                    .SetPageLength(10)
                    .SetCulture(searchModel.GetCultureFromDomains().ToLower());

                searchModel.FullTextSearchResult = _searchService.Search(search, currentPage);
            }
            else
            {
                searchModel.FullTextSearchResult = null;
            }

            return CurrentTemplate(searchModel);
        }
    }
}

```
Here I also include `IConfig` as a dependency, enabling me to get different keys from the config in my configuration. I then add a new property (`FullTextSearchResult`) to my generated model from ModelsBuilder, where I can put my search results. As you can see, the `new Search()` part is now configured in more detail. See what you can configure further below.

On this search, I enable highlighting of text in the output by using `EnableHightling()`, I then configure it to use `metaTitle` and `nodeName` as fields for getting the title of each page (that way `nodeName` is used, if `metaTitle` is empty or doesn't exist). I configure it to use `metaDescription` or the default text field for the summary. And then I set the summary length to be 160 characters (the default is 300), and the pagelength to be 10 results per page (the default is 0, meaning no pagination will occur). Lastly I set the culture, to make FullTextSearch look for results with the same culture as the request.

## Rendering the search results
```
@if (Model.FullTextSearchResult != null)
{
    <ul>
        @foreach (var result in Model.FullTextSearchResult.Results)
        {
            <li>
                <a href="@result.Url()">@result.Title</a>
                <p>
                    @result.Summary
                </p>
                <small>
                    Url: @result.Url()<br />
                    Id: @result.Id<br />
                    Last updated: @(result.Content()?.UpdateDate)<br />
                    Score: @result.Score
                </small>
            </li>
        }
    </ul>

    <div>Total results: @Model.FullTextSearchResult.TotalResults</div>

    if (Model.FullTextSearchResult.CurrentPage > 1)
    {
        <a href="?q=@Request["q"]&p=@(Model.FullTextSearchResult.CurrentPage-1)">Previous page</a>
    }

    if (Model.FullTextSearchResult.CurrentPage < Model.FullTextSearchResult.TotalPages)
    {
        <a href="?q=@Request["q"]&p=@(Model.FullTextSearchResult.CurrentPage+1)">Next page</a>
    }
}

```

Model.FullTextSearchResults now contains the result object, in which the following properties is available:

`long TotalPages`
The total number of search result pages.

`long CurrentPage`
The current search result page being viewed.

`long TotalResults`
The total number of search results.

`IEnumerable<ISearchResultItem> Results`
A list of results on the current page.

Each result is an `ISearchResultItem` with the following properties:

`string Id`
The id of the node.

`double Score`
The score given by Lucene to the result. The higher the better.

`IReadOnlyDictionary<string, string> Fields`
The fields on the item in Examine

`string Title`
The title generated by FullTextSearch according to your title properties.

`IHtmlString Summary`
The summary generated by FullTextSearch according to your summary properties and summary length.

**ISearchResultItem also comes with a few extension methods for further options:**

`string Url(this ISearchResultItem item, string culture = null)`
Gets the url of the search result item. Usage `item.Url()` or `item.Url("da-DK")`.

`string Url(this ISearchResultItem item, UrlMode mode, string culture = null)`
Gets the url of the search result item using a specific `UrlMode`. Usage `item.Url(UrlMode.Absolute)` or `item.Url(UrlMode.Absolute, "da-DK")`.

`IPublishedContent Content(this ISearchResultItem item)`
Gets the IPublishedContent for the search result.

All the extension methods are cached per request, so you don't get performance penalties for calling `Url()` or `Content()` twice on the same result.

## Configuring the search
The search service takes a `Search` object, which can be configured fluently in numerous ways. You create one by writing `new Search(string searchTerm)`, and from there you can add the following configuration:

`AddTitleProperties(string[] aliases)` or `AddTitleProperty(string alias)`
Adds field names to use for title properties. Note, that this overrides the config setting, so you need to add all wanted fields for titles here.

`RemoveTitleProperties(string[] aliases)` or `RemoveTitleProperty(string alias)`
This removes field names to use for title properties. Note that the default title properties from config is only used if no other title properties are set in the search. So you don't have to remove the default and add another if you want to swap.

`AddBodyProperties(string[] aliases)` or `AddBodyProperty(string alias)`
Adds field names to use for body properties. Note, that this overrides the config setting, so you need to add all wanted fields for bodytext here.

`RemoveBodyProperties(string[] aliases)` or `RemoveBodyProperty(string alias)`
This removes field names to use for bodyproperties. Note that the default body properties from config is only used if no other body properties are set in the search. So you don't have to remove the default and add another if you want to swap.

`AddSummaryProperties(string[] aliases)` or `AddSummaryProperty(string alias)`
Adds field names to use for summary properties. Note, that if you don't specify any summary properties, the body properties will be used instead.

`RemoveSummaryProperties(string[] aliases)` or `RemoveSummaryProperty(string alias)`
This removes field names to use for summary properties.

`AddRootNodeIds(int[] ids)` or `AddRootNodeId(int id)`
With this setting, you can limit search results to be descendants of the input ids. This way you can make the search work on specific parts of your site.

`RemoveRootNodeIds(int ids)` or `RemoveRootNodeId(int id)`
Removes already set root node ids from the search object.

`SetSummaryLength(int length)`
Sets the summary length in the results. The default is `300` characters.

`SetFuzzyness(double fuzzyness)`
Fuzzyness is used to match your search term with similar words. This method sets the fuzzyness parameter of the search. The default is `0.8`.

`EnableWildcards()` or `DisableWildcards()`
These enables or disables use of wildcards in the search terms. Wildcard characters are added automatically to each of the terms. Wildcards is disabled by default

`EnableHighlighting()` or `DisableHighlighting()`
These enables or disables search term highlighting in the search results. If enabled, search terms in the summary will be highlighted by adding `<b>` tags around them. Highlighting is disabled by default.

`SetPageLength(int length)`
This sets the desired page length of your search results. If you set it to 0, you will get all search results on the first page. The default value is 0.

`SetCulture(string culture)`
This is used to define which culture to search in. You should probably always set this, but it might work without it, in invariant sites.




