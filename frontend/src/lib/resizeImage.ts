/**
 * Zmniejszanie zdjęcia w przeglądarce przed wysłaniem (canvas -> JPEG).
 *
 * Kiedy tu zajrzeć: rozpoznawanie okładki w AddBookView.vue (limit usługi: 4 MB na plik).
 *
 * Eksport: resizeImage.
 * Backend: brak (wynik idzie do POST /api/books/recognize).
 */

/** Dłuższy bok zdjęcia po zmniejszeniu: wystarcza do odczytu tekstu, mieści się w 4 MB */
const MAX_SIDE_PX = 1600
const JPEG_QUALITY = 0.85

/** Zwraca JPEG z dłuższym bokiem najwyżej MAX_SIDE_PX (mniejsze zdjęcia tylko przekodowuje) */
export async function resizeImage(file: File, maxSide = MAX_SIDE_PX, quality = JPEG_QUALITY): Promise<Blob> {
  // imageOrientation: zdjęcia z telefonu mają obrót w EXIF; bez tego tekst mógłby być bokiem
  const bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' })
  try {
    const scale = Math.min(1, maxSide / Math.max(bitmap.width, bitmap.height))
    const width = Math.round(bitmap.width * scale)
    const height = Math.round(bitmap.height * scale)

    const canvas = document.createElement('canvas')
    canvas.width = width
    canvas.height = height
    const ctx = canvas.getContext('2d')
    if (!ctx) throw new Error('Przeglądarka nie obsługuje canvas')
    ctx.drawImage(bitmap, 0, 0, width, height)

    return await new Promise<Blob>((resolve, reject) => {
      canvas.toBlob(
        (blob) => (blob ? resolve(blob) : reject(new Error('Nie udało się przetworzyć zdjęcia'))),
        'image/jpeg',
        quality,
      )
    })
  } finally {
    bitmap.close()
  }
}
