namespace Blockcore.AtomicSwaps.Website.ThemeBase.libs;

// Core theme class
public class Theme: ITheme
{
    // Theme variables
    private bool _modeSwitchEnabled = true;

    private string _modeDefault = "light";

    private string _direction = "ltr";

    private readonly SortedDictionary<string, SortedDictionary<string, string>> _htmlAttributes = new SortedDictionary<string, SortedDictionary<string, string>>();

    private readonly SortedDictionary<string, string[]> _htmlClasses = new SortedDictionary<string, string[]>();

    // Add HTML attributes by scope
    public void addHtmlAttribute(string scope, string attributeName, string attributeValue)
    {
        SortedDictionary<string, string> attribute = new SortedDictionary<string, string>();
        if (_htmlAttributes.ContainsKey(scope))
        {
            attribute = _htmlAttributes[scope];
        }
        attribute.Add(attributeName, attributeValue);
        _htmlAttributes[scope] = attribute;
    }

    // Add HTML class by scope
    public void addHtmlClass(string scope, string className)
    {
        var list = new List<string>();
        if (_htmlClasses.ContainsKey(scope))
        {
            list = _htmlClasses[scope].ToList();
        }
        list.Add(className);
        _htmlClasses[scope] = list.ToArray();
    }

    // Print HTML attributes for the HTML template
    public string printHtmlAttributes(string scope)
    {
        var list = new List<string>();
        if (_htmlAttributes.ContainsKey(scope))
        {
            foreach (KeyValuePair<string, string> attribute in _htmlAttributes[scope])
            {
                var item = attribute.Key + "=" + attribute.Value;
                list.Add(item);
            }
            return String.Join(" ", list);
        }
        return null;
    }

    // Print HTML classes for the HTML template
    public string printHtmlClasses(string scope)
    {
        if (_htmlClasses.ContainsKey(scope))
        {
            return String.Join(" ", _htmlClasses[scope]);
        }
        return null;
    }

    public string getImageUrl(string path)
    {
        var url = path;

        return url;
    }

    // Get SVG icon content
    public string getSvgIcon(string path, string classNames)
    {
        var svg = System.IO.File.ReadAllText($"./wwwroot/{path}");

        return $"<span class=\"{classNames}\">{svg}</span>";
    }

    // Set dark mode enabled status
    public void setModeSwitch(bool flag)
    {
        _modeSwitchEnabled = flag;
    }

    // Check dark mode status
    public bool isModeSwitchEnabled()
    {
        return _modeSwitchEnabled;
    }

    // Set the mode to dark or light
    public void setModeDefault(string flag)
    {
        _modeDefault = flag;
    }

    // Get current mode
    public string getModeDefault()
    {
        return _modeDefault;
    }

    // Set style direction
    public void setDirection(string direction)
    {
       _direction = direction;
    }

    // Get style direction
    public string getDirection()
    {
        return _direction;
    }

    // Check if style direction is RTL
    public bool isRtlDirection()
    {
        return _direction.ToLower() == "rtl";
    }

    public string getAssetsFullPath(string path)
    {
        return $"/{ThemeSettings.config.AssetsDir}{path}";
    }

    // Extend CSS file name with RTL
    public string extendCssFilename(string path)
    {

        if (isRtlDirection())
        {
            path = path.Replace(".css", ".rtl.css");
        }

        return path;
    }

    // Include favicon from settings
    public string getFavicon()
    {
        return getAssetsFullPath(ThemeSettings.config.Assets.Favicon);
    }

    // Include the fonts from settings
    public string[] getFonts()
    {
        return ThemeSettings.config.Assets.Fonts.ToArray();
    }

    // Get the global assets
    public string[] getGlobalAssets(String type)
    {
        List<string> files =
            type == "Css" ? ThemeSettings.config.Assets.Css : ThemeSettings.config.Assets.Js;
        List<string> newList = new List<string>();

        foreach (string file in files)
        {
            if (type == "Css")
            {
                newList.Add(getAssetsFullPath(extendCssFilename(file)));
            }
            else
            {
                newList.Add(getAssetsFullPath(file));
            }
        }

        return newList.ToArray();
    }

    public string getAttributeValue(string scope, string attributeName){
        if (_htmlAttributes.ContainsKey(scope))
        {
            if (_htmlAttributes[scope].ContainsKey(attributeName))
            {
                return _htmlAttributes[scope][attributeName];
            }
            return "";
        }

        return "";
    }
}
