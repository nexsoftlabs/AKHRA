import { useState } from 'react'
import { Eye, EyeOff } from 'lucide-react'
import { Input, type InputProps } from '@/components/ui/input'
import { cn } from '@/lib/utils'

type PasswordInputProps = Omit<InputProps, 'type' | 'trailingIcon'>

export function PasswordInput({ className, ...props }: PasswordInputProps) {
  const [visible, setVisible] = useState(false)

  return (
    <div className={cn('relative', className)}>
      <Input
        {...props}
        type={visible ? 'text' : 'password'}
        trailingIcon={
          <button
            type="button"
            className="flex size-full items-center justify-center text-muted-foreground transition-colors hover:text-foreground focus-visible:text-primary focus-visible:outline-none"
            onClick={() => setVisible((v) => !v)}
            aria-label={visible ? 'Hide password' : 'Show password'}
            aria-pressed={visible}
            tabIndex={0}
          >
            {visible ? <EyeOff /> : <Eye />}
          </button>
        }
      />
    </div>
  )
}
