#import <UIKit/UIKit.h>
#import <CoreHaptics/CoreHaptics.h>

// Short tactile events only. The game never requests ringtone-style vibration.
static CHHapticEngine *rsEngine;
static NSMutableDictionary<NSNumber *, id<CHHapticPatternPlayer>> *rsPlayers;
static UISelectionFeedbackGenerator *rsSelection;
static UIImpactFeedbackGenerator *rsSoft;
static UINotificationFeedbackGenerator *rsNotification;
static BOOL rsEnabled = YES;
static BOOL rsSupported;
static BOOL rsRunning;
static CHHapticEvent *RSTap(double time, float intensity, float sharpness) {
    return [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticTransient parameters:@[
        [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensity],
        [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:sharpness]
    ] relativeTime:time];
}
static void RSPrepare() {
    if (!rsSelection) {
        rsSelection = [UISelectionFeedbackGenerator new];
        rsSoft = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleSoft];
        rsNotification = [UINotificationFeedbackGenerator new];
        rsSupported = [CHHapticEngine capabilitiesForHardware].supportsHaptics;
    }
    if (!rsSupported || rsEngine) return;
    NSError *error = nil;
    rsEngine = [[CHHapticEngine alloc] initAndReturnError:&error];
    if (!rsEngine) { NSLog(@"[RollerHaptics] UIKit fallback: %@",error); return; }
    rsEngine.playsHapticsOnly = YES;
    rsEngine.autoShutdownEnabled = NO;
    rsEngine.stoppedHandler = ^(CHHapticEngineStoppedReason reason) { rsRunning = NO; };
    rsPlayers = [NSMutableDictionary new];
    rsEngine.resetHandler = ^{ dispatch_async(dispatch_get_main_queue(), ^{
        [rsPlayers removeAllObjects];
        rsRunning = NO;
        if (rsEnabled) rsRunning = [rsEngine startAndReturnError:nil];
    }); };
    NSLog(@"[RollerHaptics] Core Haptics ready");
}
extern "C" {
int RSProfileRequested() { NSArray *args=[[NSProcessInfo processInfo] arguments]; return [args containsObject:@"-roller-journey-settings-screen"] ? 6 : [args containsObject:@"-roller-settings-screen"] ? 5 : [args containsObject:@"-roller-profile-screen"] ? 4 : [args containsObject:@"-roller-journey-preview"] ? 3 : [args containsObject:@"-roller-reference"] ? 2 : [args containsObject:@"-roller-profile"] ? 1 : 0; }
void RSHapticsEnable(int enabled) {
    rsEnabled = enabled != 0;
    RSPrepare();
    if (rsEnabled) { [rsSelection prepare]; [rsSoft prepare]; if (!rsRunning) rsRunning = [rsEngine startAndReturnError:nil]; }
    else { rsRunning = NO; [rsEngine stopWithCompletionHandler:nil]; }
}
void RSHapticsSuspend() { rsRunning = NO; [rsEngine stopWithCompletionHandler:nil]; }
void RSHapticsPlay(int kind) {
    if (!rsEnabled) return;
    RSPrepare();
    // Selection is the familiar precise system detent; game events use authored transients.
    if (kind == 0) { [rsSelection selectionChanged]; [rsSelection prepare]; return; }
    NSError *error = nil;
    if (rsEngine && (rsRunning || (rsRunning = [rsEngine startAndReturnError:&error]))) {
        id<CHHapticPatternPlayer> player = rsPlayers[@(kind)];
        if (!player) {
            NSArray<CHHapticEvent *> *events;
            switch(kind) {
                case 1: events = @[RSTap(0,.42,.32),RSTap(.055,.16,.65)]; break; // latch opens
                case 2: events = @[RSTap(0,.16,.68)]; break; // marble seats
                case 3: events = @[RSTap(0,.30,.40),RSTap(.065,.47,.62),RSTap(.14,.25,.80)]; break;
                case 4: events = @[RSTap(0,.34,.30),RSTap(.10,.48,.50),RSTap(.23,.64,.70)]; break;
                case 5: events = @[RSTap(0,.24,.20),RSTap(.075,.18,.15)]; break;
                default: events = @[RSTap(0,.12,.42)]; break;
            }
            CHHapticPattern *pattern = [[CHHapticPattern alloc] initWithEvents:events parameters:@[] error:&error];
            if (pattern) player = [rsEngine createPlayerWithPattern:pattern error:&error];
            if (player) rsPlayers[@(kind)] = player;
        }
        if (player && [player startAtTime:CHHapticTimeImmediate error:&error]) return;
    }
    if (kind == 4 || kind == 3) [rsNotification notificationOccurred:UINotificationFeedbackTypeSuccess];
    else { [rsSoft impactOccurredWithIntensity:kind == 2 ? .25 : .55]; [rsSoft prepare]; }
}
}
