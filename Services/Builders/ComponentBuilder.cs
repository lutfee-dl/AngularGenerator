using AngularGenerator.Core.Models;

namespace AngularGenerator.Services.Builders
{
    /// <summary>
    /// Main orchestrator that uses Builder Pattern to compose code based on user selections
    /// Supports multiple UI layouts (Table/Card) and CSS frameworks (Basic/Bootstrap/Material)
    /// </summary>
    public class ComponentBuilder
    {
        private readonly ComponentDefinition _definition;
        private TypeScriptBuilder? _tsBuilder;
        private ServiceBuilder? _serviceBuilder;
        private CssBuilder? _cssBuilder;
        
        public ComponentBuilder(ComponentDefinition definition)
        {
            _definition = definition;
        }
        
        /// <summary>
        /// Build TypeScript component using fluent API based on CRUD selections
        /// </summary>
        public string BuildTypeScript()
        {
            _tsBuilder = new TypeScriptBuilder(_definition);
            
            // Always add service
            _tsBuilder.WithService();
            
            // Always add formFields (needed for table display and forms)
            _tsBuilder.WithFormInit();
            
            // Conditionally add features based on user selections
            if (_definition.IsGet)
            {
                _tsBuilder.WithGetAll();
            }
            
            if (_definition.IsPost || _definition.IsUpdate)
            {
                _tsBuilder.WithReactiveForms();
                _tsBuilder.WithFormGroup();
            }
            
            if (_definition.IsPost)
            {
                _tsBuilder.WithCreate();
            }
            
            if (_definition.IsUpdate || _definition.IsGetById)
            {
                _tsBuilder.WithUpdate();
            }
            
            if (_definition.IsPost || _definition.IsUpdate)
            {
                _tsBuilder.WithFormSubmit();
            }
            
            if (_definition.IsDelete)
            {
                _tsBuilder.WithDelete();
            }
            
            _tsBuilder.WithNgOnInit();
            
            return _tsBuilder.Build();
        }
        
        /// <summary>
        /// Build Service TypeScript based on CRUD selections
        /// </summary>
        public string BuildService(string apiBaseUrl = "/api")
        {
            _serviceBuilder = new ServiceBuilder(_definition);
            
            // Set custom API base URL
            _serviceBuilder.WithBaseUrl(apiBaseUrl);
            
            if (_definition.IsGet)
            {
                _serviceBuilder.WithGetAll();
            }
            
            if (_definition.IsGetById)
            {
                _serviceBuilder.WithGetById();
            }
            
            if (_definition.IsPost)
            {
                _serviceBuilder.WithCreate();
            }
            
            if (_definition.IsUpdate)
            {
                _serviceBuilder.WithUpdate();
            }
            
            if (_definition.IsDelete)
            {
                _serviceBuilder.WithDelete();
            }
            
            return _serviceBuilder.Build();
        }
        
        /// <summary>
        /// Build HTML based on Layout Type (Table or Card View) and CSS Framework
        /// </summary>
        public string BuildHtml()
        {
            // เลือก Builder ตาม Layout Type
            if (_definition.LayoutType == UILayoutType.CardView)
            {
                var cardBuilder = new HtmlCardBuilder(_definition);
                return cardBuilder.Build();
            }
            else // TableView
            {
                var tableBuilder = new HtmlBuilder(_definition);
                return tableBuilder.Build();
            }
        }

        /// <summary>
        /// Build CSS file (only for BasicCSS framework)
        /// Returns empty string for Bootstrap and Angular Material
        /// </summary>
        public string BuildCss()
        {
            if (_definition.CssFramework != CSSFramework.BasicCSS)
            {
                return string.Empty; // Bootstrap และ Material ใช้ CSS จาก library
            }

            _cssBuilder = new CssBuilder(_definition);
            
            // เพิ่ม CSS ตาม Layout Type
            if (_definition.LayoutType == UILayoutType.CardView)
            {
                _cssBuilder
                    .WithCardStyles()
                    .WithButtonStyles()
                    .WithFormStyles()
                    .WithModalStyles();
            }
            else // TableView
            {
                _cssBuilder
                    .WithTableStyles()
                    .WithButtonStyles()
                    .WithFormStyles()
                    .WithModalStyles();
            }
            
            return _cssBuilder.Build();
        }
    }
}
