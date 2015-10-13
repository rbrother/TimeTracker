<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:xs="http://www.w3.org/2001/XMLSchema" 
	xmlns:fn="http://www.w3.org/2005/xpath-functions"
	xmlns:rb="http://www.brotherus.net/xslt">

	<xsl:function name="rb:date-from-dateTime" as="xs:date">
		<xsl:param name="dateTime" as="xs:dateTime"/>
		<xsl:sequence select="xs:date(substring(xs:string($dateTime),1,10))"/>
	</xsl:function>

	<xsl:function name="rb:month-from-dateTime" as="xs:string">
		<xsl:param name="dateTime" as="xs:dateTime"/>
		<xsl:sequence select="substring(xs:string($dateTime),1,7)"/>
	</xsl:function>
	
	<xsl:function name="rb:sum-of-hours" as="xs:double">
		<xsl:param name="activities"/>
		<xsl:choose>
			<xsl:when test="$activities">
				<xsl:variable name="duration" select="sum(for $act in $activities return xs:dateTime($act/EndTime) - xs:dateTime($act/StartTime) )"/>
				<xsl:sequence select="rb:total-hours($duration)"/>			
			</xsl:when>
			<xsl:otherwise>
				<xsl:sequence select="0.0"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:function>
	
	<xsl:function name="rb:total-hours" as="xs:double">
		<xsl:param name="dur" as="xs:duration"/>
		<xsl:variable name="mins" select="minutes-from-duration($dur)"/>
		<xsl:variable name="hours" select="hours-from-duration($dur)"/>
		<xsl:variable name="days" select="days-from-duration($dur)"/>
		<xsl:sequence select="$days * 24.0 + $hours + $mins div 60.0"/>
	</xsl:function>


</xsl:stylesheet>
