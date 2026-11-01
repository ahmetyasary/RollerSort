import AppKit
import AVFoundation

let args = CommandLine.arguments
guard args.count == 3 else { fatalError("usage: extract-video-contact <video> <output-dir>") }
let input = URL(fileURLWithPath: args[1])
let output = URL(fileURLWithPath: args[2], isDirectory: true)
try FileManager.default.createDirectory(at: output, withIntermediateDirectories: true)
let asset = AVURLAsset(url: input)
let duration = CMTimeGetSeconds(asset.duration)
let generator = AVAssetImageGenerator(asset: asset)
generator.appliesPreferredTrackTransform = true
generator.requestedTimeToleranceBefore = .zero
generator.requestedTimeToleranceAfter = .zero
let thumb = NSSize(width: 216, height: 468)
let columns = 4, rows = 3, perSheet = columns * rows
let count = Int(floor(duration * 2.0)) + 1
let labelStyle: [NSAttributedString.Key: Any] = [
 .font: NSFont.monospacedDigitSystemFont(ofSize: 18, weight: .bold),
 .foregroundColor: NSColor.white,
 .strokeColor: NSColor.black,
 .strokeWidth: -4
]

for sheetIndex in 0..<Int(ceil(Double(count) / Double(perSheet))) {
 let canvas = NSImage(size: NSSize(width: thumb.width * CGFloat(columns), height: thumb.height * CGFloat(rows)))
 canvas.lockFocus()
 NSColor.black.setFill(); NSRect(origin: .zero, size: canvas.size).fill()
 for slot in 0..<perSheet {
  let frameIndex = sheetIndex * perSheet + slot
  guard frameIndex < count else { continue }
  let second = Double(frameIndex) / 2.0
  guard let image = try? generator.copyCGImage(at: CMTime(seconds: second, preferredTimescale: 600), actualTime: nil) else { continue }
  let source = NSImage(cgImage: image, size: .zero)
  let column = slot % columns, row = rows - 1 - slot / columns
  let rect = NSRect(x: CGFloat(column) * thumb.width, y: CGFloat(row) * thumb.height, width: thumb.width, height: thumb.height)
  source.draw(in: rect, from: .zero, operation: .copy, fraction: 1)
  String(format: "%.1fs", second).draw(at: NSPoint(x: rect.minX + 8, y: rect.maxY - 30), withAttributes: labelStyle)
 }
 canvas.unlockFocus()
 let bitmap = NSBitmapImageRep(data: canvas.tiffRepresentation!)!
 let data = bitmap.representation(using: .jpeg, properties: [.compressionFactor: 0.86])!
 try data.write(to: output.appendingPathComponent(String(format: "contact-%02d.jpg", sheetIndex + 1)))
}
