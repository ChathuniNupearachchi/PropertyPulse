using Android.App;
using Android.Runtime;

namespace PropertyPulse.App;

// Cleartext HTTP is only allowed in Debug builds so the app can reach a development API such as http://10.0.2.2:5000.
#if DEBUG
[Application(UsesCleartextTraffic = true)]
#else
[Application]
#endif
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
