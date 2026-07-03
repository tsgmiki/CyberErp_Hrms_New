
export const HamburgerButton = ({openHandler}:{openHandler:Function}) => {

  return (
   <>
       <div className="flex m-2">
            <button
              className="text-primary hover:text-text-primary-bg-primary lg:hidden pr-2"
              aria-controls="sidebar"
              aria-expanded={false}
              onClick={(e) => {
                openHandler(true)
                e.stopPropagation()
              }}
            >
              <span className="sr-only">{'Open sidebar'}</span>
              <svg
                className="w-8 h-8 fill-current"
                viewBox="0 0 24 24"
                xmlns="http://www.w3.org/2000/svg"
              >
                <rect x="4" y="5" width="16" height="2" />
                <rect x="4" y="11" width="16" height="2" />
                <rect x="4" y="17" width="16" height="2" />
              </svg>
            </button>
          </div>
    </>
  )
}
