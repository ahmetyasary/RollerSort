# Doğrulama — 21 Ekim 2026, görsel güncelleme

Unity 6000.3.5f1 ile son kaynaklar derlendi. Son otomatik çalıştırma başarıyla tamamlandı:

`ROLLER_SORT_SMOKE_OK: six boxes released, 54 marbles matched, level won.`

Güncellenen kontrol erişilebilir kutuları seçer, kutunun gerçekten açıldığını doğrular, 54 bilyenin tamamının eşleşmesini, ödül modalının açılmasını ve altının 500'den 525'e çıkmasını bekler. Kontrol sonunda eski bölüm/altın/günlük görev kayıtları geri yüklenir. Bu değişiklik, bant dolma durumunun yanlışlıkla başarı sayılmasını da önler.

Açılış, ilk bölüm, bilye akışı, bölüm ödülü, ayarlar ve mağaza Unity Game View üzerinden kaydedildi ve görsel olarak incelendi. PNG kenar çizgileri için Clamp import, düğmeler için uygun kare/geniş varyantlar uygulandı. Telefon oranındaki çıktılar `mobile-*.png`, toplu önizlemeler `preview.png` ve `ui-preview.png` dosyalarıdır. Mobil önizlemeler gerçek Unity görüntülerinin ortalanmış ekran alanından kırpılır; ayrı maket değildir.

İleri bölümler, güçlendiriciler ve fiziksel Android/iOS cihaz performansı ayrıca test edilmedi. Piksel düzeyinde referans eşitliği iddia edilmez. Mağaza gerçek satın alma yapmayan bir tasarım önizlemesidir.

## Fiziksel iPhone açılış kontrolü

iPhone 14 Pro Max üzerinde Unity 6000.3 çıktısında görülen UIScene başlangıç çökmesi, Unity 6000.6.1f1 iOS çıktısıyla giderildi. Canlı cihaz logunda `sceneWillEnterForeground`, `sceneDidBecomeActive` ve motor başlatması doğrulandı; işlem çalışır durumda kaldı. Güncel logo simgeli paket aynı telefona yeniden kuruldu. Bu kontrol kapsamlı cihaz performans testi değildir.

## Fizik ve haptic güncellemesi

Yeni fizik, bant yuvası ve tepsi yerleşme akışıyla 54 bilyelik ilk bölüm testi exception olmadan geçti. Native Core Haptics kodu iOS SDK ile derlendi ve Xcode Release derlemesi başarılı oldu. Paket iPhone 14 Pro Max üzerine kuruldu. Son otomatik başlatma telefon kilidi nedeniyle engellendi; bu sürümün cihaz üstündeki haptic hissi henüz değerlendirilmedi.

## Bantta tek sıra ve mesafe kontrolü

22 yuvalı dairesel uçlu bant: 4096 faz boyunca dolu bant taramasında en küçük komşu merkez mesafesi 26.61908 UI birimi, boyalı bilye çapı 25.4 birim. Canlı giriş ve bant bilyeleri arasındaki mesafe kontrolü ve 54 bilyelik bölüm/ödül kontrolü birlikte geçti. Grafik arayüz testi Mac ekranı kilitlenince yavaşladığı için son tamamlanma kontrolü arayüzsüz Unity batch modunda yapıldı. `Docs/belt-detail.png`, gerçek Game View yakın görüntüsüdür.

Bant düzeltmesinin iOS Release derlemesi başarılı. Bu güncellemenin kurulum aşamasında iPhone 14 Pro Max CoreDevice tarafından unavailable olarak bildirildi; yeni paket henüz telefona kurulmadı.

Güncelleme daha sonra bağlı iPhone 14 Pro Max üzerine kuruldu ve açıldı. Cihaz logunda Core Haptics ready ve Gameplay ready doğrulandı; RollerSort işlemi çalışır durumda kaldı.
