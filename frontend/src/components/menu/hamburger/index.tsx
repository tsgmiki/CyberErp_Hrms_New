import {type ReactNode, useEffect, useRef, useState } from 'react'
import { HamburgerButton } from './button'

export default function Hamburger({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(false)
  const openHandler = (val: boolean) => {
    setOpen(val)
  }
  const dropdown = useRef(null)
  useEffect(() => {
    const clickHandler = (pros: { target: any }) => {
      if (
        !open ||
        (typeof dropdown != 'undefined' &&
          dropdown != null &&
          typeof dropdown?.current != 'undefined' &&
          dropdown?.current != null &&
          (dropdown?.current as any).contains(pros.target))
      )
        return

      openHandler(false)
    }
    document.addEventListener('click', clickHandler)
    return () => document.removeEventListener('click', clickHandler)
  })
  return (
    <div>
      <HamburgerButton openHandler={openHandler}></HamburgerButton>{' '}
      {open ? (
        <div
          ref={dropdown}
          className="z-50 absolute border-text-primary-accent ml-4 bg-secondary text-primary border-text-primary-accent border curs p-2 font-normal h-auto w-52 overflow-auto -mt-1 rounded-md"
        >
          {children}
        </div>
      ) : null}
    </div>
  )
}
