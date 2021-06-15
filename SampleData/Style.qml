<!DOCTYPE qgis PUBLIC 'http://mrcc.com/qgis.dtd' 'SYSTEM'>
<qgis version="3.4.7-Madeira" styleCategories="AllStyleCategories" minScale="1e+08" maxScale="0" hasScaleBasedVisibilityFlag="0">
  <flags>
    <Identifiable>1</Identifiable>
    <Removable>1</Removable>
    <Searchable>1</Searchable>
  </flags>
  <customproperties>
    <property value="false" key="WMSBackgroundLayer"/>
    <property value="false" key="WMSPublishDataSourceUrl"/>
    <property value="0" key="embeddedWidgets/count"/>
    <property value="Value" key="identify/format"/>
  </customproperties>
  <pipe>
    <rasterrenderer band="1" opacity="1" classificationMax="10" alphaBand="-1" classificationMin="0" type="singlebandpseudocolor">
      <rasterTransparency/>
      <minMaxOrigin>
        <limits>None</limits>
        <extent>WholeRaster</extent>
        <statAccuracy>Estimated</statAccuracy>
        <cumulativeCutLower>0.02</cumulativeCutLower>
        <cumulativeCutUpper>0.98</cumulativeCutUpper>
        <stdDevFactor>2</stdDevFactor>
      </minMaxOrigin>
      <rastershader>
        <colorrampshader clip="0" classificationMode="1" colorRampType="INTERPOLATED">
          <colorramp name="[source]" type="gradient">
            <prop k="color1" v="0,0,255,255"/>
            <prop k="color2" v="0,255,0,255"/>
            <prop k="discrete" v="0"/>
            <prop k="rampType" v="gradient"/>
          </colorramp>
          <item alpha="255" label="0" value="0" color="#c5e9fa"/>
          <item alpha="255" label="1" value="1" color="#7bbbe5"/>
          <item alpha="255" label="2" value="2" color="#488eca"/>
          <item alpha="255" label="3" value="3" color="#49a57c"/>
          <item alpha="255" label="4" value="4" color="#6fc04b"/>
          <item alpha="255" label="5" value="5" color="#d4dd57"/>
          <item alpha="255" label="6" value="6" color="#f8b849"/>
          <item alpha="255" label="7" value="7" color="#f56a29"/>
          <item alpha="255" label="8" value="8" color="#e23f28"/>
          <item alpha="255" label="9" value="9" color="#bc2741"/>
          <item alpha="255" label="10" value="10" color="#993366"/>
        </colorrampshader>
      </rastershader>
    </rasterrenderer>
    <brightnesscontrast contrast="0" brightness="0"/>
    <huesaturation saturation="0" colorizeGreen="128" grayscaleMode="0" colorizeRed="255" colorizeOn="0" colorizeBlue="128" colorizeStrength="100"/>
    <rasterresampler maxOversampling="2"/>
  </pipe>
  <blendMode>0</blendMode>
</qgis>
