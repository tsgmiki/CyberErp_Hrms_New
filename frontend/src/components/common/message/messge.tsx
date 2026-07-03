
function Message(props: { type: string; message: string }) {
  const { message } = props;

  return (
    <div>
      {props.type == "error" && (
        <div className="errorMessage">
          <div style={{ fontSize: 11, fontStyle: "italic" }}>{message}</div>
        </div>
      )}
      {props.type == "success" && (
        <div className="successMessage">
          <div style={{ fontSize: 11, fontStyle: "italic" }}>{message}</div>
        </div>
      )}
    
    </div>
  );
}

export default Message;
