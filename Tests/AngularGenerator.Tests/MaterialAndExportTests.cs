using Xunit;
using AngularGenerator.Services;
using AngularGenerator.Services.Builders;
using AngularGenerator.Services.Builders.Strategies;
using AngularGenerator.Core.Models;
using System.Collections.Generic;

namespace AngularGenerator.Tests
{
    /// <summary>
    /// Tests for features added/modified:
    ///  1. Angular Material pagination with mat-paginator (paginatedList, onPageChange, etc.)
    ///  2. PDF export (exportToPdf) with jspdf-autotable v3.x pattern
    ///  3. Status badge removal (no Active/Inactive HTML generated)
    ///  4. Material TypeScript class signature (AfterViewInit, ViewChild, MatPaginator)
    /// </summary>
    public class MaterialAndExportTests
    {
        // ─── Shared helper: build a minimal Material ComponentDefinition ──────────
        private static ComponentDefinition MakeMaterialDef(
            bool isGet = true,
            bool isPost = false,
            bool isUpdate = false,
            bool isDelete = false,
            bool hasCheckbox = false)
        {
            var fields = new List<AngularField>
            {
                new AngularField { FieldName = "ProductId", Label = "ID",    TsType = "number", UIControl = ControlType.Number, IsPrimaryKey = true },
                new AngularField { FieldName = "Name",      Label = "Name",  TsType = "string", UIControl = ControlType.Text },
                new AngularField { FieldName = "Price",     Label = "Price", TsType = "number", UIControl = ControlType.Number }
            };

            if (hasCheckbox)
                fields.Add(new AngularField { FieldName = "IsActive", Label = "Active", TsType = "boolean", UIControl = ControlType.Checkbox });

            return new ComponentDefinition
            {
                EntityName     = "Product",
                Selector       = "app-product",
                PrimaryKeyName = "ProductId",
                CssFramework   = CSSFramework.AngularMaterial,
                LayoutType     = UILayoutType.TableView,
                IsGet    = isGet,
                IsPost   = isPost,
                IsUpdate = isUpdate,
                IsDelete = isDelete,
                Fields   = fields
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SECTION 1 — TypeScript: Angular Material-specific class features
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public void Material_TS_ShouldImplementAfterViewInitAndOnInit()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("implements OnInit, AfterViewInit", ts);
        }

        [Fact]
        public void Material_TS_ShouldHaveViewChildPaginator()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("@ViewChild(MatPaginator)", ts);
            Assert.Contains("paginator!", ts);
            Assert.Contains("MatPaginator", ts);
        }

        [Fact]
        public void Material_TS_ShouldImportViewChildAndAfterViewInit()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            // @angular/core import must include all three lifecycle tokens
            Assert.Contains("ViewChild", ts);
            Assert.Contains("AfterViewInit", ts);
        }

        [Fact]
        public void Material_TS_ShouldImportMatPaginatorClassAndModule()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            // Both the module and class should be imported from @angular/material/paginator
            Assert.Contains("MatPaginatorModule", ts);
            Assert.Contains("MatPaginator", ts);
        }

        [Fact]
        public void Material_TS_ShouldHavePaginatedListComputed()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("paginatedList", ts);
            Assert.Contains("computed(", ts);
        }

        [Fact]
        public void Material_TS_CurrentPageShouldStartAtZero()
        {
            // mat-paginator is 0-indexed (pageIndex)
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("signal<number>(0)", ts);
            Assert.DoesNotContain("signal<number>(1)", ts); // must NOT be 1-indexed for Material
        }

        [Fact]
        public void Material_TS_ShouldHaveOnPageChangeMethod()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("onPageChange", ts);
            Assert.Contains("event.pageIndex", ts);
            Assert.Contains("event.pageSize", ts);
        }

        [Fact]
        public void Material_TS_ShouldHaveNgAfterViewInitWithPaginatorSubscribe()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("ngAfterViewInit", ts);
            Assert.Contains("this.paginator", ts);
            Assert.Contains("paginator.page.subscribe", ts);
        }

        [Fact]
        public void Material_TS_FilteredListShouldNotSliceData()
        {
            // For Material, filteredList() returns ALL data — mat-paginator slices
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            // paginatedList should be the one that slices, not filteredList
            Assert.Contains("paginatedList", ts);
            // filteredList should return data directly (mat-paginator handles pagination)
            Assert.Contains("return data; // mat-paginator handles pagination", ts);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SECTION 2 — HTML: Material table uses paginatedList + mat-paginator bindings
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public void Material_HTML_TableShouldUsePaginatedList()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            Assert.Contains("[dataSource]=\"paginatedList()\"", html);
            Assert.DoesNotContain("[dataSource]=\"filteredList()\"", html);
        }

        [Fact]
        public void Material_HTML_ShouldHaveMatPaginatorWithAllBindings()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            Assert.Contains("mat-paginator", html);
            Assert.Contains("[length]=\"filteredList().length\"", html);
            Assert.Contains("[pageSize]=\"pageSize()\"", html);
            Assert.Contains("[pageIndex]=\"currentPage()\"", html);
            Assert.Contains("(page)=\"onPageChange($event)\"", html);
            Assert.Contains("showFirstLastButtons", html);
        }

        [Fact]
        public void Material_HTML_ShouldHaveStickyFirstColumn()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            Assert.Contains("[sticky]=\"col.key === 'ProductId'\"", html);
        }

        [Fact]
        public void Material_HTML_ShouldHaveAtForDynamicColumns()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            Assert.Contains("@for (col of formFields; track col.key)", html);
            Assert.Contains("[matColumnDef]=\"col.key\"", html);
        }

        [Fact]
        public void Material_HTML_ShouldHaveNoDataRowWithSearchOff()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            Assert.Contains("*matNoDataRow", html);
            Assert.Contains("search_off", html);
        }

        [Fact]
        public void Material_HTML_ShouldHaveStickyHeaderRow()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            Assert.Contains("*matHeaderRowDef=\"displayedColumns; sticky: true\"", html);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SECTION 3 — Badge Removal: no Active/Inactive status badges generated
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public void Material_HTML_ShouldNotContainStatusBadge_WhenHasCheckbox()
        {
            var def     = MakeMaterialDef(isGet: true, hasCheckbox: true);
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            // Badges/chips should NOT appear in the table view
            Assert.DoesNotContain("Active", html);
            Assert.DoesNotContain("Inactive", html);
            Assert.DoesNotContain("mat-chip", html);
            Assert.DoesNotContain("badge-active", html);
            Assert.DoesNotContain("badge-inactive", html);
        }

        [Fact]
        public void Bootstrap_HTML_Table_ShouldNotContainStatusBadge_WhenHasCheckbox()
        {
            var def = new ComponentDefinition
            {
                EntityName     = "Order",
                PrimaryKeyName = "OrderId",
                CssFramework   = CSSFramework.Bootstrap,
                LayoutType     = UILayoutType.TableView,
                IsGet          = true,
                Fields         = new List<AngularField>
                {
                    new AngularField { FieldName = "OrderId",  IsPrimaryKey = true, TsType = "number", UIControl = ControlType.Number },
                    new AngularField { FieldName = "IsActive", TsType = "boolean",  UIControl = ControlType.Checkbox }
                }
            };
            var builder = new ComponentBuilder(def);
            var html    = builder.BuildHtml();

            Assert.DoesNotContain("badge-active", html);
            Assert.DoesNotContain("badge-inactive", html);
            Assert.DoesNotContain("Active", html);
            Assert.DoesNotContain("Inactive", html);
        }

        [Fact]
        public void BasicCSS_GetBadgeClass_ShouldReturnEmptyString()
        {
            var renderer = new BasicCssRenderer();

            Assert.Equal(string.Empty, renderer.GetBadgeClass(true));
            Assert.Equal(string.Empty, renderer.GetBadgeClass(false));
        }

        [Fact]
        public void Material_GetBadgeClass_ShouldReturnEmptyString()
        {
            var renderer = new MaterialCssRenderer();

            Assert.Equal(string.Empty, renderer.GetBadgeClass(true));
            Assert.Equal(string.Empty, renderer.GetBadgeClass(false));
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SECTION 4 — PDF Export: exportToPdf method in generated TypeScript
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public void Material_TS_ShouldContainExportToPdfMethod()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("exportToPdf", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldUseAutoTableV3StandaloneFunction()
        {
            // jspdf-autotable v3.x: autoTable(doc, options) — NOT doc.autoTable(...)
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("autoTable(doc,", ts);
            Assert.DoesNotContain("doc.autoTable(", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldUseLandscapeOrientation()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("orientation: 'landscape'", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldUseGridTheme()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("theme: 'grid'", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldShowHeadEveryPage()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("showHead: 'everyPage'", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldHaveDidDrawPageCallback()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("didDrawPage", ts);
            Assert.Contains("getNumberOfPages", ts);       // page count
            Assert.Contains("toLocaleDateString('th-TH')", ts); // Thai locale date
        }

        [Fact]
        public void Material_TS_ExportToPdf_FilenameContainsTimestamp()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            // File saved with timestamp: _report_ + getTime() + .pdf
            Assert.Contains("getTime()", ts);
            Assert.Contains(".pdf", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldUseDataList_NotPaginatedList()
        {
            // PDF must export ALL records — not just current page
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("this.dataList()", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldHaveErrorHandling()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("catch (err", ts);
            Assert.Contains("Cannot find module", ts);
            Assert.Contains("jspdf@^2.5.2", ts);
            Assert.Contains("jspdf-autotable@^3.8.3", ts);
        }

        [Fact]
        public void Material_TS_ExportToPdf_ShouldUseSequentialAwaitImports()
        {
            // Must NOT use Promise.all — sequential await is more reliable with Angular bundler
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("await import('jspdf')", ts);
            Assert.Contains("await import('jspdf-autotable')", ts);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SECTION 5 — Excel Export: exportToExcel method
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public void Material_TS_ShouldContainExportToExcelMethod()
        {
            var def     = MakeMaterialDef(isGet: true);
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("exportToExcel", ts);
            Assert.Contains("import('xlsx')", ts);
            Assert.Contains("XLSX.writeFile", ts);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SECTION 6 — Non-Material: Basic/Bootstrap pagination still uses 1-indexed
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public void Bootstrap_TS_CurrentPageShouldStartAtOne()
        {
            var def = new ComponentDefinition
            {
                EntityName     = "Order",
                PrimaryKeyName = "OrderId",
                CssFramework   = CSSFramework.Bootstrap,
                LayoutType     = UILayoutType.TableView,
                IsGet          = true,
                Fields         = new List<AngularField>
                {
                    new AngularField { FieldName = "OrderId", IsPrimaryKey = true, TsType = "number", UIControl = ControlType.Number }
                }
            };
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("signal<number>(1)", ts);
            Assert.DoesNotContain("paginatedList", ts); // Bootstrap doesn't need paginatedList
        }

        [Fact]
        public void Bootstrap_TS_ShouldNotHaveAfterViewInit()
        {
            var def = new ComponentDefinition
            {
                EntityName     = "Order",
                PrimaryKeyName = "OrderId",
                CssFramework   = CSSFramework.Bootstrap,
                LayoutType     = UILayoutType.TableView,
                IsGet          = true,
                Fields         = new List<AngularField>
                {
                    new AngularField { FieldName = "OrderId", IsPrimaryKey = true, TsType = "number", UIControl = ControlType.Number }
                }
            };
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.DoesNotContain("AfterViewInit", ts);
            Assert.DoesNotContain("ngAfterViewInit", ts);
            Assert.DoesNotContain("@ViewChild", ts);
        }

        [Fact]
        public void Bootstrap_TS_FilteredListShouldSliceData()
        {
            // For Basic/Bootstrap: filteredList() handles pagination slice itself
            var def = new ComponentDefinition
            {
                EntityName     = "Order",
                PrimaryKeyName = "OrderId",
                CssFramework   = CSSFramework.Bootstrap,
                LayoutType     = UILayoutType.TableView,
                IsGet          = true,
                Fields         = new List<AngularField>
                {
                    new AngularField { FieldName = "OrderId", IsPrimaryKey = true, TsType = "number", UIControl = ControlType.Number }
                }
            };
            var builder = new ComponentBuilder(def);
            var ts      = builder.BuildTypeScript();

            Assert.Contains("currentPage() - 1) * this.pageSize()", ts);
        }
    }
}
