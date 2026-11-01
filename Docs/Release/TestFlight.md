# Roller Sort TestFlight

Bundle ID: `com.smartlogy.rollersort`. Team: Ahmet Yasar YILDIRIM (`9VSUBY2W33`). App Store Connect app ID: `6825489802`.

Xcode must be signed in under Settings → Apple Accounts. Export with automatic signing and `-allowProvisioningUpdates`. The successful export uses the existing **Cloud Managed Apple Distribution** certificate expiring 22 July 2027; a locally stored distribution private key is not required. Do not create replacement certificates merely because `security find-identity` does not list this cloud certificate.

Successful archive: `Builds/TestFlight/RollerSort-smartlogy-1.0-1.xcarchive`.
IPA: `Builds/TestFlight/Export-Smartlogy-Cloud/RollerSort.ipa`.
Version/build: `1.0 (1)`.
Export settings: `Builds/TestFlight/ExportOptions.plist`.
Export log: `Builds/testflight-cloud-export.log`.
Signature evidence: `Builds/TestFlight/Export-Smartlogy-Cloud/DistributionSummary.plist`.

```sh
xcodebuild -exportArchive -archivePath Builds/TestFlight/RollerSort-smartlogy-1.0-1.xcarchive -exportOptionsPlist Builds/TestFlight/ExportOptions.plist -exportPath Builds/TestFlight/Export-Smartlogy-Cloud -allowProvisioningUpdates
```

Transporter provider must be Ahmet Yasar YILDIRIM. Upload status is verified separately from successful IPA creation.

Transporter confirmed **Delivered** on 21 October 2026 at22:59 Europe/Istanbul. Apple status at verification: **The app is processing**. TestFlight processing and tester availability are separate from successful delivery.

Latest locally exported archive: `Builds/TestFlight/RollerSort-smartlogy-1.0-2.xcarchive`.
Latest IPA: `Builds/TestFlight/Export-Smartlogy-Cloud-1.0-2/RollerSort.ipa`.
Latest version/build: `1.0 (2)`. Export verified on 22 October 2026 with the existing Cloud Managed Apple Distribution certificate (`3960CAF8AF3D4533134F4F61B26BFB2613479145`) and App Store provisioning profile for `com.smartlogy.rollersort`. This build has not been uploaded by this export step.


27 October 2026: release candidate **1.0 (5)** exported successfully with existing cloud signing and delivered through Transporter. IPA: `Builds/TestFlight/Export-Smartlogy-Cloud-1.0-5/RollerSort.ipa`. App Store review submission verified **Waiting for Review** at 22:13 Europe/Istanbul; see `AppStoreSubmission.md`.
