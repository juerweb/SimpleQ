using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SimpleQ.Droid
{
    public class PowerSaverManager
    {
    private static List<Intent> POWERMANAGER_INTENTS = new List<Intent>()
    {
        new Intent().SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity")),
        new Intent().SetComponent(new ComponentName("com.letv.android.letvsafe", "com.letv.android.letvsafe.AutobootManageActivity")),
        new Intent().SetComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity")),
        new Intent().SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity")),
        new Intent().SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.startupapp.StartupAppListActivity")),
        new Intent().SetComponent(new ComponentName("com.oppo.safe", "com.oppo.safe.permission.startup.StartupAppListActivity")),
        new Intent().SetComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.AddWhiteListActivity")),
        new Intent().SetComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.BgStartUpManager")),
        new Intent().SetComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity")),
        new Intent().SetComponent(new ComponentName("com.asus.mobilemanager", "com.asus.mobilemanager.entry.FunctionActivity")).SetData(Android.Net.Uri.Parse("mobilemanager://function/entry/AutoStart"))
    };

            public static void StartPowerSaverIntent(Context context)
            {
                ISharedPreferences settings = context.GetSharedPreferences("ProtectedApps", FileCreationMode.Private);
                bool skipMessage = settings.GetBoolean("skipAppListMessage", false);
                if (!skipMessage)
                {
                    ISharedPreferencesEditor editor = settings.Edit();
                    foreach (Intent intent in POWERMANAGER_INTENTS)
                    {
                        if (context.PackageManager.ResolveActivity(intent, PackageInfoFlags.MatchDefaultOnly) != null)
                        {
                            var dontShowAgain = new Android.Support.V7.Widget.AppCompatCheckBox(context);
                            dontShowAgain.Text = "Do not show again";
                            dontShowAgain.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
                            {
                                editor.PutBoolean("skipAppListMessage", e.IsChecked);
                                editor.Apply();
                            };

                            new AlertDialog.Builder(context)
                            .SetIcon(Android.Resource.Drawable.IcDialogAlert)
                            .SetTitle(string.Format("Add {0} to list", context.GetString(Resource.String.library_name)))
                            .SetMessage(string.Format("{0} requires to be enabled/added in the list to function properly.\n", context.GetString(Resource.String.library_name)))
                            .SetView(dontShowAgain)
                            .SetPositiveButton("Go to settings", (o, d) => context.StartActivity(intent))
                            .SetNegativeButton(Android.Resource.String.Cancel, (o, d) => { })
                            .Show();

                            break;
                        }
                    }
                }
            }
        }
}