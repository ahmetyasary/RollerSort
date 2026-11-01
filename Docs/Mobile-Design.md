# Roller Sort — Mobil tasarım yönü

## Ekran dili

Dikey 450 × 975 referans alanı, Android/iOS güvenli alanına ölçekleme. Mavi oyun zemini, kalın lacivert çerçeveler, mor üst HUD, parlak sarı/pembe/yeşil/turkuaz/mor/mavi bilyeler. Logo altın sarısı ROLLER ve turkuaz SORT metninden oluşur. Ekranlarda özel SVG kaynaklarından üretilmiş PNG sprite seti, ImageGen logo/hediye varlıkları ve Lilita One yazı tipi kullanılır.

## Çekirdek döngü

1. Oyuncu bir kolondaki en alt erişilebilir kutuyu açar.
2. Dokuz bilye kısa aralıklarla huniden banda iner.
3. Bilyeler bantta dolaşır ve rengi eşleşen tepsiye girer.
4. Üç bilye dolunca tepsi tamamlanır ve sıradaki açılır.
5. Tüm kutular ve tepsiler bittiğinde 25 altın kazanılır.

Bantta 36 bilye tutulabilir; 11. bölümde açılan yedek alan bunu o bölüm için 54'e yükseltir. Dolu bant 18 saniye eşleşme yapamazsa yeniden deneme ekranı çıkar. Başarısızlığı önlemek için kutuların yanında tepsi kuyrukları da okunmalıdır.

## Mevcut bölüm eğrisi

- Bölüm 1: 2 kolon × 3 sıra, 3 renk.
- Bölüm 2: 2 × 3, 3 renk.
- Bölüm 3: 3 × 3.
- Bölüm 4: 4 kolon, 4/4/2/2 yükseklikli basamaklı düzen.
- Bölüm 5+: 4/4/2/2 kolon yükseklikleri; renk sayısı 6'ya kadar artar.

Bu üretici başlangıç doğrulaması içindir; sınırlı çeşitlilik sağlar. Yayına hazır uzun bir oyun için tek başına yeterli değildir.

## Sonraki içerik planı

İlk 20 bölümde kademeli öğretim; ardından gizli renk, donmuş kutu, anahtar/kilit, çok katlı kutu ve değişen tepsi düzenleri. Her yeni özellik önce tek başına öğretilmeli, sonra bilinenlerle birleştirilmelidir. Bölüm verileri oyun kodundan ayrılmalı ve her bölüm çözülebilirlik kontrolünden geçmelidir.

Günlük görev mevcut: 3 bölüm bitir, 100 altın al. Sonraki aşama: haftalık harita, isteğe bağlı kısa etkinlikler, oyuncunun bıraktığı yerden sürdürme, erişilebilir ses ve titreşim kontrolü. Yaşam sınırı ve zorunlu reklam ilk prototipte yoktur.

## Değerlendirme

Uzun oynanış hedefine ulaşmayı gerçek oyun testleri belirler. İlk ölçümler: öğreticiyi bitirme, bölüm bazında başarı/yeniden deneme, oyuncunun nerede durduğu, tepsi kuyruğunu anlayıp anlamadığı ve tekrar oynamayı isteyip istemediği. Cihaz profillemesinde 60 FPS hedefi, bellek, batarya ve arka plana alıp dönme davranışı incelenmelidir.
