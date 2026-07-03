const getParseContent = (language:string,content:string) => {
  const contentList = content?.split(";:#");
  let newContent = "";
  contentList?.map((item) => {
    const itemList = item?.split(",,,");
    const oldlang = itemList[0];
    const oldContent = itemList[1];
    if (oldlang == language) {
      newContent = oldContent;
    }
  });
  return newContent;
};
const parseContent = (props:{language:string;newEditorState?: any;content:any}) => {
  const{newEditorState,language,content}=props
  const contentList = content?.split(';:#')
  let newContent = ''
  let notExist=true;
  contentList?.map((item: any) => {
    const itemList = item?.split(',,,')
    const oldlang = itemList[0]
    const oldContent = itemList[1]
    if (oldlang == language) {
      const langContent = oldlang + ',,,' +(typeof newEditorState!='undefined'? (newEditorState as any).toHTML():oldContent) 
      newContent = newContent != '' ? newContent+';:#'+ langContent : langContent
      notExist=false
    } else {
      const langContent = oldlang + ',,,' + oldContent
      newContent = newContent != '' ? newContent+';:#' + langContent : langContent
    }
  })
  if(notExist && typeof newEditorState!='undefined')
  {
    const langContent = language + ',,,' +  (newEditorState as any).toHTML()
    newContent = newContent != '' ?newContent+ ';:#' + langContent : langContent
  }
  return newContent;
}
const parseValue= (props:{language:string;newValue?: any;content:any}) => {
  const{newValue,language,content}=props
  const contentList = content?.split(';:#')
  let newContent = ''
  let notExist=true;
  contentList?.map((item: any) => {
    const itemList = item?.split(',,,')
    const oldlang = itemList[0]
    const oldContent = itemList[1]
    if (oldlang == language) {
      const langContent = oldlang + ',,,' +(typeof newValue!='undefined'? newValue:oldContent) 
      newContent = newContent != '' ? newContent+';:#'+ langContent : langContent
      notExist=false
    } else {
      const langContent = oldlang + ',,,' + oldContent
      newContent = newContent != '' ? newContent+';:#' + langContent : langContent
    }
  })
  if(notExist && typeof newValue!='undefined')
  {
    const langContent = language + ',,,' +  newValue
    newContent = newContent != '' ?newContent+ ';:#' + langContent : langContent
  }
  return newContent;
}
const getCurrentState = (content: any,lang:any) => {
  const contentList = content?.split(';:#')
  const newContent =
    getContent(contentList, lang) != ""
      ? getContent(contentList, lang)
      : getContent(contentList, 'en') != ""
        ? getContent(contentList, 'en')
        : content;
  return newContent;
}
const getContent=(contentList:any[],lang:any)=>{
  let newContent = ''
  contentList?.map((item: any) => {
    const itemList = item?.split(',,,')
    const oldlang = itemList[0]
    const oldContent = itemList[1]
    if (oldlang == lang) {
      newContent = oldContent
    }
  })
  return newContent
}
export  {getParseContent,parseContent,getCurrentState,parseValue};
