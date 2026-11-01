# Hareket, malzeme ve haptic — 21 Ekim 2026

## Haptic

`Handheld.Vibrate` kaldırıldı. iOS'ta native `CHHapticEngine` ve kısa `hapticTransient` olayları kullanılır. Hareketler ses ayarından bağımsızdır; HAPTICS düğmesi bütün dokunsal olayları kapatır. Destek olmayan cihazlarda UIKit'in selection/soft impact/success geri bildirimleri kullanılır; uzun telefon titreşimine dönülmez.

| Olay | His tasarımı |
| --- | --- |
| Düğme | Sistem selection detenti |
| Kutu açılması | Yumuşak klik + 55 ms sonra çok hafif ikinci darbe |
| Bilye yerleşmesi | Düşük yoğunluklu kısa dokunuş; çok sık olaylar sınırlanır |
| Tepsi tamamlanması | 140 ms içinde üç kısa darbe |
| Bölüm tamamlanması | 230 ms içinde yükselen üç kısa darbe |
| Engelli hamle | İki hafif, daha yumuşak darbe |

Core Haptics motoru kesintilerden sonra tekrar hazırlanır; arka plana alındığında durdurulur. Native pattern player'lar önbelleklenir. Android'de `View.performHapticFeedback` ile semantik dokunma olayları kullanılır; Android cihaz testi henüz yapılmadı.

Apple kaynakları: [Core Haptics](https://developer.apple.com/documentation/CoreHaptics), [Transient events](https://developer.apple.com/documentation/corehaptics/chhapticevent/eventtype/haptictransient).

## Hareket

Bilyelerin huni içindeki hareketi 1/120 saniyelik sabit alt adımlarla hesaplanır: yerçekimi, hız sönümü, eğimli huni yüzeylerine çarpma, esneklik ve eşit kütleli bilye temasları. Bu UI koordinatlarında çalışan özel bir 2B çözücüdür; Rigidbody kullanılmaz. Bantta bilye çapına göre aralıklandırılmış 22 ayrı hareketli yuva vardır. Dairesel uçlu bant yolu yay uzunluğuna göre ilerler; dolu banttaki komşu merkezlerin en küçük mesafesi 26.62, görsel bilye çapı 25.4 UI birimidir. Girişte boş yuva beklenir; bilyeler yerleştikleri yuvayla birlikte ilerler. Tepsiye alınan bilye 220 ms süren kısa bir yay çizer.

Bilye görselleri yeniden kullanılır. Uçuşta temas gölgesi zayıflar ve hızla çok hafif uzama oluşur. Parlama dünya ışığı yönünü korur. Kutu kapağı kalkarak kaybolur, kaynak kutu sıkışıp söner, hedefe gelen bilye küçük bir yaylanmayla oturur. Dolu tepsi 420 ms içinde çıkar; kuyruk 230 ms içinde yükselip yerleşir. Sweet/Neat kısa süreli büyüme ve solma kullanır. Parıltı parçacıkları, düğme basma yayı, modal giriş ve hediye salınımı eklendi.

## Assetler

`Tools/generate-quality-art.mjs` yedi renkte yeni bilye, kapak, kaynak kutusu ve hedef tepsi; ayrı temas gölgesi, parıltı ve haptic dokunma ikonu üretir. Dosyalar `Assets/Resources/Art/*-v2.png`, düzenlenebilir SVG kaynakları `Assets/RollerSort/ArtSource/` içindedir. Eski assetler korunur.

Yeniden üretme: `node Tools/generate-art.mjs`, ardından `node Tools/generate-quality-art.mjs`.

## Doğrulama

İlk bölüm otomatik oyunda 54 bilyenin tamamı eşleşti; ödül ekranı ve 25 altın doğrulandı. Test oyun kodundaki çalışma zamanı exception'larını da hata kabul eder. Native Objective-C++ dosyası iOS SDK ile syntax kontrolünden geçti. Fiziksel haptic hissinin yoğunluğu ve ritmi gerçek oyuncu geri bildirimiyle ayrıca ayarlanmalıdır.

## Tek sıralı bant düzeltmesi

Bant girişi 140 ms süren, boş yuvaya zamanlanmış tekli besleme kullanır. Bütün taşıma hareketleri aynı sabit simülasyon saatini izler; düşük FPS sırasında bant ile giriş animasyonu birbirinden kopmaz. Hunide bekleyen bilyeler bant çizgisinin üzerine taşamaz. Tepsiye ayrılan bilye önce aşağı iner ve küçülerek yuvasına yerleşir. `BeltSpacingCheck`, tümü dolu bandı 4096 konum boyunca tarar; oynanış testi giriş ve bant bilyeleri arasında da canlı mesafe kontrolü yapar.
