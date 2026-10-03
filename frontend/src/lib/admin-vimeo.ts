import { parseVimeoVideoId } from '@/lib/vimeo'

/** Normalize admin form input to a Vimeo numeric id for the API, or null if empty. */
export function vimeoFieldToApiValue(raw: string | null | undefined): string | null {
  const trimmed = raw?.trim()
  if (!trimmed) {
    return null
  }
  const id = parseVimeoVideoId(trimmed)
  if (!id) {
    throw new Error('Enter a Vimeo numeric id or a vimeo.com URL.')
  }
  return id
}
