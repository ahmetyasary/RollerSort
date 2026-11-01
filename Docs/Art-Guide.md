# Roller Sort — Görsel varlıklar

21 Ekim 2026 tasarım geçişi. Referans ekranlardaki dikey oranlar 450 × 975 koordinat sistemine aktarıldı.

## Dosyalar

- `Assets/Resources/Art/`: oyun içinde kullanılan şeffaf PNG varlıklar (SVG kaynaklarının 3 katı çözünürlük).
- `Assets/RollerSort/ArtSource/`: düzenlenebilir SVG kaynaklar.
- `Assets/Resources/RollerSortLogoV2.png`: yeni Roller Sort logosu, ImageGen ile üretildi.
- `Assets/Resources/Art/gift-v2.png`: parlak mor/altın ödül kutusu, ImageGen ile üretildi.
- `Assets/Resources/Fonts/LilitaOne.ttf`: Lilita One yuvarlak oyun yazı tipi; SIL OFL lisansı aynı klasörde.
- `Tools/generate-art.mjs`: geometrik varlıkların üreticisi; tekrar üretmek için proje kökünden `npm ci --prefix Tools` ve `node Tools/generate-art.mjs`.

## Varlık seti

Yedi renkte bilye, kapalı kutu kapağı, dokuzlu kaynak kutusu ve üç gözlü hedef tepsi; iki/üç/dört kolonlu basamaklı oyun hazneleri; alt gövde; oval bant ve bant yuvası; tepsi rayı; mor HUD ve düğme; sarı/yeşil/turuncu/kırmızı düğmeler; ayar dişlisi; kilit; altın; artı; kapatma; ses/titreşim; mıknatıs; karıştırma; bilye makinesi; kalp; mağaza tentesi; ödül diyaloğu; ilerleme çubuğu; uygulama ikonu ve açılış arka planı.

PNG import ayarları ArtImport tarafından UI için uygulanır: Clamp, mipmap kapalı, alfa kenar düzeltmesi, sıkıştırmasız. Font ve ikonlar metin glifi yerine gerçek varlıklar kullanır.

## Ekranlar

Oynanışta üst HUD, 6/8/11 kilitleri, geniş mavi hazne, daralan açık renk huni, ince oval bant ve dört uzun tepsi kuyruğu referans oranlarına göre düzenlendi. İlk iki bölümdeki kutu renk düzeni referanstaki iki kolonlu ekrana uyar.

Açılış ekranı mavi-turkuaz geçiş ve orta logo kullanır. Ödül ekranında mor panel, parlak hediye, dolum çubuğu ve yeşil Continue düğmesi bulunur. Ayarlar büyük turuncu simge düğmelerini, mağaza pembe tenteyi ve paket kartlarını kullanır. Mağaza kartları önizlemedir; gerçek para tahsil edilmez.

Bu çalışma yeniden çizilmiş görsellerdir; referansın özgün oyun dosyaları kullanılmadı. Piksel düzeyinde birebir eşleşme ölçülmedi. Bilye akışı ve tepsi tamamlanması çalışır; özgün oyunun tüm animasyon zamanlamalarının eşleştiği iddia edilmez.

## Uygulama simgesi

`Assets/Resources/Art/app-icon-logo.png` splash ekranındaki Roller Sort logosunun mavi arka planlı uygulama simgesi sürümüdür. MobileArtSetup bu dosyayı varsayılan uygulama ikonu olarak kullanır. Eski sarı bilye ikonu kaynak olarak korunur.
