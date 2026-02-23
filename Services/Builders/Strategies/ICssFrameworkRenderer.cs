namespace AngularGenerator.Services.Builders.Strategies
{
    public interface ICssFrameworkRenderer
    {
        string GetContainerClass();
        string GetButtonClass(string variant = "primary");
        string GetTableClass();
        string GetBadgeClass(bool isActive);
        string GetInputClass();
        string RenderButton(string text, string onClick, string? icon = null);
        string RenderFormInput(string fieldKey, string fieldType, int? maxLength = null);
        string[] GetRequiredImports();
        string GetImportsDeclaration();
        bool RequiresSpecialTableRendering();
    }
}