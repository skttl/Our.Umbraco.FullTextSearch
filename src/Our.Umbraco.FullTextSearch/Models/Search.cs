using Lucene.Net.QueryParsers;
using Our.Umbraco.FullTextSearch.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Our.Umbraco.FullTextSearch.Models
{
    public class Search : ISearch
    {
        public Search(string searchTerm)
        {
            SearchType = SearchType.MultiRelevance;
            SearchTerm = searchTerm;
            TitleProperties = new string[] { };
            TitleBoost = 10.0;
            BodyProperties = new string[] { };
            SummaryProperties = new string[] { };
            RootNodeIds = new int[] { };
            SummaryLength = 300;
            PageLength = 0;
            Fuzzyness = 0.8;
            AddWildcard = false;
            AllowedContentTypes = new string[] { };
        }

        public string Culture { get; set; }
        public SearchType SearchType { get; set; }
        public string SearchTerm { get; set; }
        public string[] TitleProperties { get; set; }
        public double TitleBoost { get; set; }
        public string[] BodyProperties { get; set; }
        public string[] SummaryProperties { get; set; }
        public int[] RootNodeIds { get; set; }
        public int SummaryLength { get; set; }
        public int PageLength { get; set; }
        public double Fuzzyness { get; set; }
        public bool AddWildcard { get; set; }
        public bool HighlightSearchTerms { get; set; }
        public string[] AllowedContentTypes { get; set; }

        public ICollection<string> SearchTermQuoted => new List<string> { '"' + QueryParser.Escape(SearchTerm) + '"' };

        public ICollection<string> SearchTermSplit => new List<string> { QueryParser.Escape(SearchTerm) };

        public Search SetSearchType(SearchType searchType)
        {
            SearchType = searchType;
            return this;
        }

        #region TitleProperties
        public Search AddTitleProperties(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                AddTitleProperty(alias);
            }

            return this;
        }

        public Search AddTitleProperty(string alias)
        {
            if (!TitleProperties.Contains(alias)) TitleProperties = TitleProperties.Append(alias).ToArray();
            return this;
        }

        public Search RemoveTitleProperties(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                RemoveTitleProperty(alias);
            }

            return this;
        }

        public Search RemoveTitleProperty(string alias)
        {
            if (!TitleProperties.Contains(alias)) TitleProperties = TitleProperties.Where(x => x != alias).ToArray();
            return this;
        }

        public Search SetTitleBoost(double titleBoost)
        {
            TitleBoost = titleBoost;
            return this;
        }
        #endregion

        #region BodyProperties
        public Search AddBodyProperties(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                AddBodyProperty(alias);
            }

            return this;
        }

        public Search AddBodyProperty(string alias)
        {
            if (!BodyProperties.Contains(alias)) BodyProperties = BodyProperties.Append(alias).ToArray();
            return this;
        }

        public Search RemoveBodyProperties(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                RemoveBodyProperty(alias);
            }

            return this;
        }

        public Search RemoveBodyProperty(string alias)
        {
            if (!BodyProperties.Contains(alias)) BodyProperties = BodyProperties.Where(x => x != alias).ToArray();
            return this;
        }
        #endregion

        #region SummaryProperties
        public Search AddSummaryProperties(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                AddSummaryProperty(alias);
            }

            return this;
        }

        public Search AddSummaryProperty(string alias)
        {
            if (!SummaryProperties.Contains(alias)) SummaryProperties = SummaryProperties.Append(alias).ToArray();
            return this;
        }

        public Search RemoveSummaryProperties(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                RemoveSummaryProperty(alias);
            }

            return this;
        }

        public Search RemoveSummaryProperty(string alias)
        {
            if (!SummaryProperties.Contains(alias)) SummaryProperties = SummaryProperties.Where(x => x != alias).ToArray();
            return this;
        }
        #endregion

        #region RootNodes
        public Search AddRootNodeIds(int[] ids)
        {
            foreach (var id in ids)
            {
                AddRootNodeId(id);
            }

            return this;
        }

        public Search AddRootNodeId(int id)
        {
            if (!RootNodeIds.Contains(id)) RootNodeIds = RootNodeIds.Append(id).ToArray();
            return this;
        }

        public Search RemoveRootNodeIds(int[] ids)
        {
            foreach (var id in ids)
            {
                RemoveRootNodeId(id);
            }

            return this;
        }

        public Search RemoveRootNodeId(int id)
        {
            if (!RootNodeIds.Contains(id)) RootNodeIds = RootNodeIds.Where(x => x != id).ToArray();
            return this;
        }
        #endregion

        public Search SetSummaryLength(int length)
        {
            SummaryLength = length;
            return this;
        }

        public Search SetFuzzyness(double fuzzyness)
        {
            Fuzzyness = fuzzyness;
            return this;
        }

        public Search EnableWildcards()
        {
            AddWildcard = true;
            return this;
        }

        public Search DisableWildcards()
        {
            AddWildcard = false;
            return this;
        }

        public Search EnableHighlighting()
        {
            HighlightSearchTerms = true;
            return this;
        }

        public Search DisableHighlighting()
        {
            HighlightSearchTerms = false;
            return this;
        }

        public Search SetPageLength(int length)
        {
            PageLength = length;
            return this;
        }

        public Search SetCulture(string culture)
        {
            Culture = culture;
            return this;
        }

        public Search AddAllowedContentTypes(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                AddAllowedContentType(alias);
            }

            return this;
        }

        public Search AddAllowedContentType(string alias)
        {
            if (!AllowedContentTypes.Contains(alias)) AllowedContentTypes = AllowedContentTypes.Append(alias).ToArray();
            return this;
        }

        public Search RemoveAllowedContentTypes(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                RemoveAllowedContentType(alias);
            }

            return this;
        }

        public Search RemoveAllowedContentType(string alias)
        {
            if (!AllowedContentTypes.Contains(alias)) AllowedContentTypes = AllowedContentTypes.Where(x => x != alias).ToArray();
            return this;
        }
    }
}
