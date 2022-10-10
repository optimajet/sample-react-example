import {Button, ButtonGroup} from "rsuite";
import React from "react";

const SchemeMenu = (props) => {
    const onClick = () => {
        const newCode = prompt('Enter scheme name');
        if (newCode) {
            props.onNewScheme?.(newCode);
        }
    }

    return <ButtonGroup>
        <Button disabled={true}>Scheme name: {props.schemeCode}</Button>
        <Button onClick={onClick}>Create or load scheme</Button>
        <Button onClick={() => props.onCreateProcess?.()}>Create process</Button>
    </ButtonGroup>
}

export default SchemeMenu;
