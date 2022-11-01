namespace Blockcore.AtomicSwaps.Website.ThemeBase.libs;

// Core theme interface
public interface ITheme
{
    void addHtmlAttribute(string scope, string attributeName, string attributeValue);
    void addHtmlClass(string scope, string className);

    string printHtmlAttributes(string scope);

    string printHtmlClasses(string scope);

    string getImageUrl(string path);

    string getSvgIcon(string path, string classNames);

    void setModeSwitch(bool flag);
    
    bool isModeSwitchEnabled();
    
    void setModeDefault(string flag);

    string getModeDefault();

    void setDirection(string direction);

    string getDirection();

    bool isRtlDirection();

    string getAssetsFullPath(string path);

    string extendCssFilename(string path);

    string getFavicon();

    string[] getFonts();

    string[] getGlobalAssets(String type);

    string getAttributeValue(string scope, string attributeName);
}
