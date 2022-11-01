using Blockcore.AtomicSwaps.Website.ThemeBase.libs;

namespace Blockcore.AtomicSwaps.Website.ThemeBase;

public interface IBootstrapBase
{
    void initThemeMode();
    
    void initThemeDirection();
    
    void initRtl();

    void initLayout();

    void init(ITheme theme);
}