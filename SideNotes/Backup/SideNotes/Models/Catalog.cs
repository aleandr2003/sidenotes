using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models
{
    public class Catalog
    {
        private List<Category> allCategories;

        private List<Category> rootCategories;
        public List<Category> RootCategories
        {
            get
            {
                if (rootCategories == null)
                {
                    rootCategories = new List<Category>();
                    rootCategories.AddRange(BuildTree(allCategories));
                }
                return rootCategories;
            }
        }

        private List<CatalogEntry> index;
        public List<CatalogEntry> Index
        {
            get
            {
                if (index == null)
                {
                    index = new List<CatalogEntry>();
                    BuildIndexBranch(index, RootCategories, 0);
                }
                return index;
            }
        }

        public Catalog(List<Category> categories)
        {
            allCategories = categories;
            rootCategories = new List<Category>();
            rootCategories.AddRange(BuildTree(categories));

            index = new List<CatalogEntry>();
            BuildIndexBranch(index, rootCategories, 0);
        }

        private List<Category> BuildTree(List<Category> categories)
        {
            foreach (var category in categories.Where(c => c.ParentId != null))
            {
                var parent = categories.FirstOrDefault(c => c.Id == category.ParentId);
                if (parent != null)
                {
                    parent.Children.Add(category);
                    category.Parent = parent;
                }
            }
            return categories.Where(c => c.Parent == null).ToList();
        }

        private void BuildIndexBranch(List<CatalogEntry> _index, List<Category> branches, int depth)
        {
            foreach (var category in branches)
            {
                _index.Add(new CatalogEntry() { category = category, depth = depth });
                BuildIndexBranch(_index, category.Children, depth + 1);
            }
        }

        public List<Category> GetDescendants(int CatId)
        {
            var category = allCategories.FirstOrDefault(c => c.Id == CatId);
            if (category == null) return null;

            List<CatalogEntry> _index = new List<CatalogEntry>();
            BuildIndexBranch(_index, category.Children, 0);
            return _index.Select(e => e.category).ToList();
        }
    }

    public class CatalogEntry
    {
        public Category category;
        public int depth;
    }
}