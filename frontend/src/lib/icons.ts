/** Mapowanie kluczy UI → identyfikatory Iconify (Lucide). */
export const formIcons = {
  book: 'lucide:book',
  mail: 'lucide:mail',
  lock: 'lucide:lock',
  eye: 'lucide:eye',
  eyeOff: 'lucide:eye-off',
  at: 'lucide:at-sign',
  pin: 'lucide:map-pin',
  check: 'lucide:check',
  xmark: 'lucide:x',
  users: 'lucide:users',
  repeat: 'lucide:repeat',
  star: 'lucide:star',
} as const

export type FormIconName = keyof typeof formIcons

export const toastIcons = {
  success: 'lucide:check',
  error: 'lucide:alert-circle',
} as const
