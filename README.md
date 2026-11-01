# Roller Sort

Android ve iOS hedefli Unity 6 (6000.6.1f1) dikey mobil oyun prototipi.

## Çalıştırma

Unity Hub üzerinden bu klasörü açın. `Assets/RollerSort/Scenes/RollerSort.unity` sahnesinde Play'e basın. Sahne eksikse `Roller Sort > Prepare Mobile Project` menüsü sahneyi ve mobil ayarları oluşturur. Game görünümünde 450 × 975 veya dikey bir telefon çözünürlüğü seçin.

## Mevcut kapsam

- Roller Sort logosu, animasyonlu yükleme ekranı.
- Dokunarak açılan kutular; dokuz bilyenin dökülmesi ve oval bantta dolaşması.
- Renk eşleştiren üç gözlü tepsiler, sıradaki tepsi kuyruğu ve tamamlanma animasyonu.
- Bölüme göre artan kolon, sıra ve renk sayısı; tekrarlanabilir bölüm üretimi.
- Bant kapasitesi, dolma durumu ve yeniden deneme; 15. bölümden itibaren genişletilmiş bant (22 yerine 26 yuva).
- 9. bölümden itibaren tüneller, taş engeller; Hard / Very Hard bölümleri.
- Harita (journey) ekranı, koleksiyon rozetleri, profil/avatar düzenleme.
- Can sistemi: en fazla 5 can, 30 dakikada bir yenilenir.
- Bölüm ödülü (50–175 altın), beş bölümlük hediye göstergesi, cihazda ilerleme kaydı, üç bölüm için günlük 250 altın görevi.
- 6/8/11. bölümlerde açılan mıknatıs, kutu karıştırma ve temizleme güçlendiricileri; üç adeti 1.800 altın.
- İngilizce ve Türkçe arayüz (`Assets/Resources/Localization`), ses/titreşim ayarları, cihazın güvenli ekran alanına uyum.

Bilyeler hunide 120 Hz sabit adımlı özel 2B temas/yerçekimi çözücüsüyle hareket eder; bantta ayrı yuvalar, tepsiye girişte kısa yerleşme animasyonu kullanılır. Rigidbody kullanılmaz. Görseller referansların genel düzenini temel alan ilk uygulamadır; birebir görsel eşleşme tamamlanmış değildir.

## Mobil derleme

Build Profiles üzerinden Android veya iOS seçin. iOS modülü bu makinede mevcut. Android için Unity Hub'dan aynı Editor sürümüne Android Build Support, SDK/NDK ve OpenJDK kurulmalıdır. iOS için Xcode takım ve imzalama ayarları gerekir. iOS 1.0 (5) sürümü 27 Ekim 2026'da App Store incelemesine gönderildi; ayrıntılar `Docs/Release/` içindedir.

## Açık işler

Gerçek IAP/reklam SDK entegrasyonu, editörle tasarlanmış ve oynanış testleriyle dengelenmiş bölüm kataloğu, zorluk eğrisi, erişilebilirlik, Android mağaza yayını ve görsel ince ayar. Yayındaki sürüm ücretsizdir; gerçek para ile satın alma ve reklam içermez. Mağaza ekranı satın alma yapmayan bir önizlemedir. Altın ve ilerleme `PlayerPrefs` içinde tutulur; gerçek satın alma eklenmeden önce sunucu doğrulaması gerekir. Beş bölümlük hediye göstergesi görsel prototiptir; ayrı hediye ekonomisi henüz bağlı değildir.

Saatlerce oynanabilirlik bir garanti değildir; bölüm tamamlama oranı, tekrar deneme oranı ve oyuncu geri bildirimleriyle değerlendirilmelidir.

## Kontrol

Editor batch smoke kontrolü, ilk bölümün altı kutusunu sırayla açar; 54 bilyenin eşleşip bölümün tamamlanmasını doğrular. `RollerSort.Editor.SmokeCheck.Run` giriş noktası bu amaçla kullanılır. Bu kontrol ilerleme kaydını sıfırlar; yalnız test verisiyle çalıştırın.

## Görsel güncelleme

Referans oranlarında oyun haznesi, huni, bant, uzun tepsi kuyrukları, kapaklar, bilyeler, HUD, ayarlar, mağaza ve ödül ekranı için özel varlık seti eklendi. Düzenlenebilir kaynaklar ve yeniden üretme adımları `Docs/Art-Guide.md` içindedir. Yeni ekran görüntüleri `Docs/` klasöründedir.

## iOS açılış uyumluluğu

Proje Unity 6000.6.1f1 kullanır. iOS 27 SDK ile 6000.3 çıktısı UIScene yaşam döngüsü kontrolünde açılışta kapanıyordu. Güncel Unity iOS çıktısındaki UnityScene ve UIApplicationSceneManifest desteği kullanılır.

## Hareket ve haptic kalitesi

Native iOS Core Haptics, Android semantik dokunma geri bildirimi, 31 yeni malzeme/efekt asseti, bilye havuzu, tepsi/kutu/düğme animasyonları eklendi. Ayrıntılar `Docs/Feel-Quality.md` içindedir.
