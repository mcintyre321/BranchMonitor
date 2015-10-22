
class SirenField extends React.Component {
  constructor(props) {
    super(props);
    this.state = props;
   
  }
  
  render() {
	<div> some field </div>
	//var state = this.state;
    //return <input type={state.type} value={state.value} name={state.name} />;
  }
}

class SirenAction extends React.Component {
  constructor(props) {
    super(props);
    this.state = props;
   
  }
  
  render() {

	return (
		<form action={this.state.href} method={this.state.method}>
			<input value={this.state.name} type="submit" disabled={!this.state.enabled}/>
		</form>
	)
	
  }
}
 
class Siren extends React.Component {
  constructor(props) {
    super(props);
    this.state = props;
   
  }
  
  render() {
	var state = this.state;
	var classes = this.state.class || []
	classes.push("entity");
	var classNamesAtt = classes.join(' ');
    return (
      <div className={classNamesAtt}>
		{
			Object.keys(state.properties).map(function(key) {
				return <div key={key}>{key}: {state.properties[key]}</div>;
			})
		}
		{
			(state.entities || []).map(function(e) {
				return <Siren {...e} />;
			})
		}
		{
			(state.actions || []).map(function(e) {
				return <SirenAction {...e} />;
			})
		}
      </div>
    )
  }
}
