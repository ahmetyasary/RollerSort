#import <UIKit/UIKit.h>
#import <QuartzCore/QuartzCore.h>
#import "UI/UnityScene.h"
#import "UnityAppController.h"

static UIWindow *rsLaunchWindow;
@interface RollerLaunchScene : UnityScene { UIWindow *_rollerWindow; }
@property(nonatomic,strong) UIWindow *window;
@end
@implementation RollerLaunchScene
@synthesize window = _rollerWindow;
- (void)scene:(UIScene *)scene willConnectToSession:(UISceneSession *)session options:(UISceneConnectionOptions *)options {
    [super scene:scene willConnectToSession:session options:options];
    if (![scene isKindOfClass:UIWindowScene.class]) return;
    if (!self.window) {
        self.window = [[UIWindow alloc] initWithWindowScene:(UIWindowScene *)scene];
        self.window.rootViewController = [[UIStoryboard storyboardWithName:@"RollerLaunchScreen" bundle:NSBundle.mainBundle] instantiateInitialViewController];
    }
    rsLaunchWindow = self.window;
    rsLaunchWindow.windowLevel = UIWindowLevelNormal + 1;
    rsLaunchWindow.userInteractionEnabled = NO;
    [rsLaunchWindow makeKeyAndVisible];
    [rsLaunchWindow layoutIfNeeded];
    [CATransaction flush];
    NSLog(@"[RollerLaunch] Branded launch visible");
}
@end
extern "C" void RSHideLaunchScreen() {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (!rsLaunchWindow) return;
        id delegate = rsLaunchWindow.windowScene.delegate;
        rsLaunchWindow.hidden = YES;
        rsLaunchWindow.rootViewController = nil;
        if ([delegate isKindOfClass:RollerLaunchScene.class]) ((RollerLaunchScene *)delegate).window = GetAppController().window;
        rsLaunchWindow = nil;
        [GetAppController().window makeKeyAndVisible];
        NSLog(@"[RollerLaunch] Unity splash ready");
    });
}
